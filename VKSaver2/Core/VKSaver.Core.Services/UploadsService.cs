using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services.Database;
using VKSaver.Core.Services.Interfaces;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Web;
using static VKSaver.Core.Models.Common.FileContentTypeExtensions;

namespace VKSaver.Core.Services
{
    public sealed class UploadsService : IUploadsService, ISuspendingService
    {
        public event EventHandler<TransferItem> ProgressChanged;
        public event EventHandler<TransferOperationErrorEventArgs> UploadError;
        public event EventHandler UploadsCompleted;

        public UploadsService(ILogService logService, 
            IUploadsPostprocessor uploadsPostprocessor)
        {
            _logService = logService;
            _uploadsPostprocessor = uploadsPostprocessor;

            _transferGroup = BackgroundTransferGroup.CreateGroup(UPLOAD_TRASNFER_GROUP_NAME);
            _transferGroup.TransferBehavior = BackgroundTransferBehavior.Serialized;
            _uploads = new List<UploadOperation>(INIT_DOWNLOADS_LIST_CAPACITY);
            _cts = new Dictionary<Guid, CancellationTokenSource>(INIT_DOWNLOADS_LIST_CAPACITY);
        }

        public bool IsLoading { get; private set; }

        public async void StartService()
        {
            lock (_lockObject)
            {
                if (_isRunning)
                    return;
                _isRunning = true;
            }

            if (_database == null)
                _database = await UploadsDatabase.CreateDatabaseAsync();

            TryFinishUncompletedUploads();
            DiscoverActiveUploads();
        }

        public void StopService()
        {
            lock (_lockObject)
            {
                if (!_isRunning)
                    return;
                _isRunning = false;
            }
        }

        public void Cancel(Guid operationGuid)
        {
            CancellationTokenSource cts = null;
            if (!_cts.TryGetValue(operationGuid, out cts))
                return;
            try
            {
                cts.Cancel();
            }
            catch (Exception) { }
        }

        public Task CancelAllAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    var guids = _cts.Keys.ToList();
                    foreach (var g in guids)
                        _cts[g].Cancel();
                }
                catch (Exception) { }
            });
        }

        public async void DiscoverActiveUploads()
        {
            lock (_lockObject)
            {
                if (_uploadsAttached)
                    return;
                _uploadsAttached = true;
            }

            IsLoading = true;            

            await Task.Run(async () =>
            {
                IReadOnlyList<UploadOperation> uploads = null;

                try { uploads = await BackgroundUploader.GetCurrentUploadsForTransferGroupAsync(_transferGroup); }
                catch (Exception ex)
                {
                    WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);
                    _logService.LogText($"Getting uploads error - {error}\n{ex.ToString()}");
                }

                if (uploads != null && uploads.Count > 0)
                {
                    foreach (var upload in uploads)
                    {                        
                        HandleUploadAsync(upload, false);
                    }
                }
            });

            IsLoading = false;
        }

        public IEnumerable<TransferItem> GetAllUploads()
        {
            if (_uploads.Count == 0)
                return null;

            return _uploads.Select(e => new TransferItem
            {
                OperationGuid = e.Guid,
                Name = GetOperationNameFromFile(e.SourceFile),
                ContentType = GetContentTypeFromExtension(e.SourceFile.FileType),
                Status = e.Progress.Status,
                TotalSize = FileSize.FromBytes(e.Progress.TotalBytesToReceive),
                ProcessedSize = FileSize.FromBytes(e.Progress.BytesReceived)
            });
        }

        public int GetUploadsCount()
        {
            return _uploads.Count;
        }

        public async Task<List<UploadInitError>> StartUploadingAsync(IList<IUpload> uploads)
        {
            var errors = new List<UploadInitError>();
            foreach (var upload in uploads)
            {
                if (_uploads.Count == INIT_DOWNLOADS_LIST_CAPACITY)
                {
                    errors.Add(new UploadInitError(UploadInitErrorType.MaxUploadsExceeded, upload));
                    continue;
                }

                string field = null;
                if (upload.Uploadable.ContentType == FileContentType.Video)
                    field = "video_file";
                else
                    field = "file";

                string boundary = String.Format(BOUNDARY_MASK, DateTime.Now.Ticks.ToString("x"));
                string header = String.Format(HEADER_MASK, boundary, field,
                    upload.Uploadable.Name, upload.Uploadable.Source.GetContentType());
                string footer = String.Format(FOOTER_MASK, boundary);

                var tempFile = await CreateTemporaryFile(upload, header, footer);
                if (tempFile == null)
                {
                    errors.Add(new UploadInitError(UploadInitErrorType.CantPrepareData, upload));
                    continue;
                }

                try
                {
                    var operation = await CreateUploadOperationAsync(tempFile, upload, boundary);
                    HandleUploadAsync(operation, true);
                }
                catch (Exception ex)
                {
                    _logService.LogException(ex);
                    await tempFile.DeleteAsync(StorageDeleteOption.PermanentDelete);

                    errors.Add(new UploadInitError(UploadInitErrorType.Unknown, upload));
                    continue;
                }
            }

            return errors;
        }

        private async Task<StorageFile> CreateTemporaryFile(IUpload upload, string header, string footer)
        {
            StorageFile file = null;

            try
            {
                var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(
                    UPLOADER_TEMP_FOLDER, CreationCollisionOption.OpenIfExists);

                file = await folder.CreateFileAsync(
                    upload.Uploadable.Name, CreationCollisionOption.GenerateUniqueName);

                using (var fileStream = await file.OpenStreamForWriteAsync())
                using (var strWritter = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    strWritter.Write(header);
                    strWritter.Flush();

                    var sourceStream = await upload.Uploadable.Source.GetDataStreamAsync();
                    await sourceStream.CopyToAsync(fileStream);

                    strWritter.Write(footer);
                    strWritter.Flush();
                }

                return file;
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
                await file?.DeleteAsync(StorageDeleteOption.PermanentDelete);
                return null;
            }
            finally
            {
                upload.Uploadable.Source?.Dispose();                
            }
        }

        private async Task<UploadOperation> CreateUploadOperationAsync(StorageFile file, IUpload upload, string boundary)
        {
            var uploader = new BackgroundUploader();
            uploader.TransferGroup = _transferGroup;
            uploader.SetRequestHeader("Content-Type",
                String.Format(REQUEST_HEADER_CONTENT_TYPE_MASK, boundary));

            var operation = uploader.CreateUpload(new Uri(upload.UploadUrl), file);

            AttachNotifications(uploader, upload);

            var completedUpload = new CompletedUpload
            {
                Id = operation.Guid,
                Name = upload.Uploadable.Name,
                ContentType = upload.Uploadable.ContentType
            };
            await _database.InsertAsync(completedUpload);

            return operation;
        }

        private void AttachNotifications(BackgroundUploader uploader, IUpload upload)
        {

        }

        private async void HandleUploadAsync(UploadOperation operation, bool start)
        {
            var tokenSource = new CancellationTokenSource();
            var callback = new Progress<UploadOperation>(OnUploadProgressChanged);
            _uploads.Add(operation);
            _cts.Add(operation.Guid, tokenSource);

            OnUploadProgressChanged(operation);

            try
            {
                if (start)
                    await operation.StartAsync().AsTask(tokenSource.Token, callback);
                else
                    await operation.AttachAsync().AsTask(tokenSource.Token, callback);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logService.LogException(ex);
                OnUploadError(operation, ex);
            }

            FinishUpload(operation);
        }

        private async void FinishUpload(UploadOperation operation)
        {
            if (operation.Progress.Status == BackgroundTransferStatus.Completed)
            {
                try
                {
                    string serverResponse = await (new StreamReader(
                    operation.GetResultStreamAt(0).AsStreamForRead())).ReadToEndAsync();

                    var cachedUpload = await _database.GetAsync(operation.Guid);
                    var newUpload = new CompletedUpload
                    {
                        Id = cachedUpload.Id,
                        Name = cachedUpload.Name,
                        ContentType = cachedUpload.ContentType,
                        ServerResponse = serverResponse
                    };

                    await _database.InsertOrReplaceAsync(newUpload);
                    var result = await _uploadsPostprocessor.ProcessUploadAsync(newUpload);

                    if (result != UploadsPostprocessorResultType.ConnectionError)
                        await _database.RemoveAsync(operation.Guid);
                }
                catch (Exception ex)
                {
                    _logService.LogException(ex);
                }
            }
            else
                await _database.RemoveAsync(operation.Guid);

            OnUploadProgressChanged(operation);

            _cts.Remove(operation.Guid);
            _uploads.Remove(operation);

            try
            {
                await operation.SourceFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception) { }

            if (_uploads.Count == 0)
                UploadsCompleted?.Invoke(this, EventArgs.Empty);
        }

        private async void TryFinishUncompletedUploads()
        {
            try
            {
                var uploads = await _database.GetNotCompletedAsync();
                foreach (var upload in uploads)
                {
                    var result = await _uploadsPostprocessor.ProcessUploadAsync(upload);
                    if (result != UploadsPostprocessorResultType.ConnectionError)
                        await _database.RemoveAsync(upload.Id);
                }
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
            }
        }

        private void OnUploadProgressChanged(UploadOperation e)
        {
            ProgressChanged?.Invoke(this, new TransferItem
            {
                OperationGuid = e.Guid,
                Name = GetOperationNameFromFile(e.SourceFile),
                ContentType = GetContentTypeFromExtension(e.SourceFile.FileType),
                Status = e.Progress.Status,
                TotalSize = FileSize.FromBytes(e.Progress.TotalBytesToReceive),
                ProcessedSize = FileSize.FromBytes(e.Progress.BytesReceived)
            });
        }

        private void OnUploadError(UploadOperation e, Exception ex)
        {
            UploadError?.Invoke(this, new TransferOperationErrorEventArgs(
                    e.Guid,
                    GetOperationNameFromFile(e.SourceFile),
                    GetContentTypeFromExtension(e.SourceFile.FileType),
                    ex));
        }

        private string GetOperationNameFromFile(IStorageFile file)
        {
            return file.Name.Split(new char[] { '.' })[0];
        }

        private bool _isRunning;
        private bool _uploadsAttached;
        private UploadsDatabase _database;

        private readonly BackgroundTransferGroup _transferGroup;
        private readonly List<UploadOperation> _uploads;
        private readonly Dictionary<Guid, CancellationTokenSource> _cts;        

        private readonly ILogService _logService;
        private readonly IUploadsPostprocessor _uploadsPostprocessor;

        private readonly object _lockObject = new object();

        private const string UPLOAD_TRASNFER_GROUP_NAME = "VKSaverUploader";
        private const string UPLOADER_TEMP_FOLDER = "UploaderTemp";
        private const string BOUNDARY_MASK = "VKSaver 2 {0}";
        private const string HEADER_MASK = "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n";
        private const string FOOTER_MASK = "\r\n--{0}--\r\n";
        private const string REQUEST_HEADER_CONTENT_TYPE_MASK = "multipart/form-data; boundary={0}";
        private const int INIT_DOWNLOADS_LIST_CAPACITY = 30;
    }
}
