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
using NotificationsExtensions.ToastContent;
using Windows.UI.Notifications;
using VKSaver.Core.Services.Common;
using Windows.Data.Xml.Dom;

namespace VKSaver.Core.Services
{
    public sealed class UploadsService : IUploadsService, ISuspendingService
    {
        public event EventHandler<TransferItem> ProgressChanged;
        public event EventHandler<TransferOperationErrorEventArgs> UploadError;
        public event EventHandler UploadsCompleted;

        public UploadsService(
            ILogService logService, 
            IUploadsPostprocessor uploadsPostprocessor,
            ILocService locService)
        {
            _logService = logService;
            _uploadsPostprocessor = uploadsPostprocessor;
            _locService = locService;

            _transferGroup = BackgroundTransferGroup.CreateGroup(UPLOAD_TRASNFER_GROUP_NAME);
            _transferGroup.TransferBehavior = BackgroundTransferBehavior.Serialized;
            _uploads = new Dictionary<UploadOperation, ICompletedUpload>(INIT_DOWNLOADS_LIST_CAPACITY);
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
                        var cachedUpload = await _database.GetAsync(upload.Guid);           
                        HandleUploadAsync(upload, cachedUpload, false);
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
                OperationGuid = e.Key.Guid,
                Name = e.Value.Name,
                ContentType = e.Value.ContentType,
                Status = e.Key.Progress.Status,
                TotalSize = FileSize.FromBytes(e.Key.Progress.TotalBytesToReceive),
                ProcessedSize = FileSize.FromBytes(e.Key.Progress.BytesReceived)
            });
        }

        public int GetUploadsCount()
        {
            return _uploads.Count;
        }

        public async Task<UploadInitError> StartUploadingAsync(IUpload upload)
        {
            if (_uploads.Count == INIT_DOWNLOADS_LIST_CAPACITY)
                return new UploadInitError(UploadInitErrorType.MaxUploadsExceeded, upload);

            string field = null;
            if (upload.Uploadable.ContentType == FileContentType.Video)
                field = "video_file";
            else
                field = "file";

            string boundary = String.Format(BOUNDARY_MASK, DateTime.Now.Ticks.ToString("x"));

            var part = CreateContentPart(upload, field);
            if (part == null)
                return new UploadInitError(UploadInitErrorType.CantPrepareData, upload);

            try
            {
                var tuple = await CreateUploadOperationAsync(new List<BackgroundTransferContentPart>(1) { part }, upload, boundary);
                HandleUploadAsync(tuple.Item1, tuple.Item2, true);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
                return new UploadInitError(UploadInitErrorType.Unknown, upload);
            }

            return null;
        }

        public Task<List<UploadInitError>> StartUploadingAsync(IList<IUpload> uploads)
        {
            throw new NotImplementedException();
        }

        private BackgroundTransferContentPart CreateContentPart(IUpload upload, string fieldName)
        {
            var part = new BackgroundTransferContentPart(fieldName, upload.Uploadable.Source.GetFile().Path);
            part.SetHeader("Content-Type", PART_CONTENT_TYPE);
            part.SetFile(upload.Uploadable.Source.GetFile());
            return part;
        }
        
        private async Task<Tuple<UploadOperation, ICompletedUpload>> CreateUploadOperationAsync(
            IEnumerable<BackgroundTransferContentPart> parts, IUpload upload, string boundary)
        {
            var uploader = new BackgroundUploader();
            uploader.TransferGroup = _transferGroup;

            AttachNotifications(uploader, upload);

            var operation = await uploader.CreateUploadAsync(new Uri(upload.UploadUrl), parts, "form-data", boundary);

            var completedUpload = new CompletedUpload
            {
                Id = operation.Guid,
                Name = upload.Uploadable.Name,
                ContentType = upload.Uploadable.ContentType
            };
            await _database.InsertAsync(completedUpload);

            return new Tuple<UploadOperation, ICompletedUpload>(operation, completedUpload);
        }

        private void AttachNotifications(BackgroundUploader uploader, IUpload upload)
        {
            var successToast = ToastContentFactory.CreateToastText02();
            successToast.Audio.Content = ToastAudioContent.SMS;
            successToast.TextHeading.Text = _locService["Toast_Uploads_SuccessReturn_Text"];
            successToast.TextBodyWrap.Text = upload.Uploadable.Name;

            var successXml = successToast.GetXml();
            ToastAudioHelper.SetSuccessAudio(successXml);

            var successNotification = new ToastNotification(successXml);

            var failToast = ToastContentFactory.CreateToastText02();
            failToast.Audio.Content = ToastAudioContent.IM;
            failToast.TextHeading.Text = _locService["Toast_Uploads_Fail_Text"];
            failToast.TextBodyWrap.Text = upload.Uploadable.Name;

            var failXml = failToast.GetXml();
            ToastAudioHelper.SetFailAudio(failXml);

            var failNotification = new ToastNotification(failXml);

            uploader.SuccessToastNotification = successNotification;
            uploader.FailureToastNotification = failNotification;
        }

        private async void HandleUploadAsync(UploadOperation operation, ICompletedUpload upload, bool start)
        {
            var tokenSource = new CancellationTokenSource();
            var callback = new Progress<UploadOperation>(o => OnUploadProgressChanged(o, upload));
            _uploads.Add(operation, upload);
            _cts.Add(operation.Guid, tokenSource);

            OnUploadProgressChanged(operation, upload);

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
                OnUploadError(operation, upload, ex);
            }

            FinishUpload(operation, upload);
        }

        private async void FinishUpload(UploadOperation operation, ICompletedUpload upload)
        {
            if (operation.Progress.Status == BackgroundTransferStatus.Completed)
            {
                try
                {
                    string serverResponse = await (new StreamReader(
                    operation.GetResultStreamAt(0).AsStreamForRead())).ReadToEndAsync();
                    
                    var newUpload = new CompletedUpload
                    {
                        Id = upload.Id,
                        Name = upload.Name,
                        ContentType = upload.ContentType,
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

            OnUploadProgressChanged(operation, upload);

            _cts.Remove(operation.Guid);
            _uploads.Remove(operation);

            if (_uploads.Count == 0)
                UploadsCompleted?.Invoke(this, EventArgs.Empty);
        }

        private async void TryFinishUncompletedUploads()
        {
            await Task.Run(async () =>
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
            });
        }

        private void OnUploadProgressChanged(UploadOperation e, ICompletedUpload upload)
        {
            ProgressChanged?.Invoke(this, new TransferItem
            {
                OperationGuid = e.Guid,
                Name = upload.Name,
                ContentType = upload.ContentType,
                Status = e.Progress.Status,
                TotalSize = FileSize.FromBytes(e.Progress.TotalBytesToSend),
                ProcessedSize = FileSize.FromBytes(e.Progress.BytesSent)
            });
        }

        private void OnUploadError(UploadOperation e, ICompletedUpload upload, Exception ex)
        {
            UploadError?.Invoke(this, new TransferOperationErrorEventArgs(
                    e.Guid,
                    upload.Name,
                    upload.ContentType,
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
        private readonly Dictionary<UploadOperation, ICompletedUpload> _uploads;
        private readonly Dictionary<Guid, CancellationTokenSource> _cts;        

        private readonly ILogService _logService;
        private readonly IUploadsPostprocessor _uploadsPostprocessor;
        private readonly ILocService _locService;

        private readonly object _lockObject = new object();

        private const string UPLOAD_TRASNFER_GROUP_NAME = "VKSaverUploader";
        private const string UPLOADER_TEMP_FOLDER = "UploaderTemp";
        private const string BOUNDARY_MASK = "VKSaver 2 {0}";
        private const string PART_CONTENT_TYPE = "application/octet-stream";
        private const int INIT_DOWNLOADS_LIST_CAPACITY = 30;
    }
}
