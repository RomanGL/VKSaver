using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Services.Transfer;
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
        }

        public bool IsLoading { get; private set; }

        public void StartService()
        {
            lock (_lockObject)
            {
                if (_isRunning)
                    return;
                _isRunning = true;
            }

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
                    for (int i = 0; i < uploads.Count; i++)
                        HandleUploadAsync(uploads[i], false);
            });

            IsLoading = false;
        }

        public IEnumerable<TransferItem> GetAllUploads()
        {
            throw new NotImplementedException();
        }

        public int GetUploadsCount()
        {
            return _uploads.Count;
        }

        public Task<List<UploadInitError>> StartUploadingAsync(IList<IUpload> uploads)
        {
            throw new NotImplementedException();
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

        private readonly BackgroundTransferGroup _transferGroup;
        private readonly List<UploadOperation> _uploads;
        private readonly Dictionary<Guid, CancellationTokenSource> _cts;

        private readonly ILogService _logService;
        private readonly IUploadsPostprocessor _uploadsPostprocessor;

        private readonly object _lockObject = new object();

        private const string UPLOAD_TRASNFER_GROUP_NAME = "VKSaverUploader";
    }
}
