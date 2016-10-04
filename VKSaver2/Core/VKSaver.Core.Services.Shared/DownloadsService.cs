using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Services.Transfer;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Web;
using static VKSaver.Core.Services.Common.DownloadsExtensions;
using static VKSaver.Core.Models.Common.FileContentTypeExtensions;
using System.IO;
using Newtonsoft.Json;
using NotificationsExtensions.ToastContent;
using VKSaver.Core.Services.Common;
using Windows.UI.Notifications;

namespace VKSaver.Core.Services
{
    /// <summary>
    /// Представляет сервис для рабоы с загрузками.
    /// </summary>
    public sealed class DownloadsService : IDownloadsService, ISuspendingService
    {        
        /// <summary>
        /// Происходит при изменении прогресса загрузки.
        /// </summary>
        public event EventHandler<TransferItem> ProgressChanged;
        /// <summary>
        /// Происходит при возникновении серьезной ошибки загрузки.
        /// </summary>
        public event EventHandler<TransferOperationErrorEventArgs> DownloadError;
        /// <summary>
        /// Происходит при завершении всех загрузок.
        /// </summary>
        public event EventHandler DownloadsCompleted;

        public DownloadsService(
            IMusicCacheService musicCacheService, 
            ISettingsService settingsService,
            ILogService logService,
            ILocService locService)
        {
            _musicCacheService = musicCacheService;
            _settingsService = settingsService;
            _logService = logService;
            _locService = locService;

            _transferGroup = BackgroundTransferGroup.CreateGroup(DOWNLOADS_TRANSFER_GROUP_NAME);
            _transferGroup.TransferBehavior = BackgroundTransferBehavior.Serialized;
            _downloads = new List<DownloadOperation>(INIT_DOWNLOADS_LIST_CAPACITY);
            _cts = new Dictionary<Guid, CancellationTokenSource>(INIT_DOWNLOADS_LIST_CAPACITY);
            _musicDownloads = new Dictionary<string, VKSaverAudio>();
        }

        /// <summary>
        /// Выполняется ли в данный момент загрузка текущих загрузок.
        /// </summary>
        public bool IsLoading { get; private set; }

        /// <summary>
        /// Возвращает массив со всеми выполняющимися загрузками.
        /// </summary>
        public TransferItem[] GetAllDownloads()
        {
            if (_downloads.Count == 0) return null;
            return _downloads.Select(e => new TransferItem
            {
                OperationGuid = e.Guid,
                Name = GetOperationNameFromFile(e.ResultFile),
                ContentType = GetContentTypeFromExtension(e.ResultFile.FileType),
                Status = e.Progress.Status,
                TotalSize = FileSize.FromBytes(e.Progress.TotalBytesToReceive),
                ProcessedSize = FileSize.FromBytes(e.Progress.BytesReceived)
            }).ToArray();
        }

        public int GetDownloadsCount()
        {
            return _downloads.Count;
        }

        /// <summary>
        /// Найти все фоновые загрузки и обработать их.
        /// </summary>
        public async void DiscoverActiveDownloads()
        {
            lock (_lockObject)
            {
                if (_downloadsAttached)
                    return;
                _downloadsAttached = true;
            }

            IsLoading = true;

            await Task.Run(async () =>
            {
                IReadOnlyList<DownloadOperation> downloads = null;

                try { downloads = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(_transferGroup); }
                catch (Exception ex)
                {
                    WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);
                    _logService.LogText($"Getting downloads error - {error}\n{ex.ToString()}");
                }

                if (downloads != null && downloads.Count > 0)
                    for (int i = 0; i < downloads.Count; i++)
                        HandleDownloadAsync(downloads[i], false);
            });

            IsLoading = false;
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

        public void PauseResume(Guid operationGuid)
        {
            var download = _downloads.FirstOrDefault(d => d.Guid == operationGuid);
            if (download == null)
                return;

            try
            {
                if (download.Progress.Status.IsPaused())
                    download.Resume();
                else if (download.Progress.Status.IsRunning())
                    download.Pause();
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

        public async void StartService()
        {
            lock (_lockObject)
            {
                if (_isRunning)
                    return;
                _isRunning = true;
            }

            if (!_downloadsAttached)
            {
                var stream = await GetMetadataFileAsync(false);
                var reader = new StreamReader(stream);
                using (var jsonReader = new JsonTextReader(reader))
                {

                    var serializer = new JsonSerializer();
                    var mDownloads = serializer.Deserialize<Dictionary<string, VKSaverAudio>>(jsonReader);

                    if (mDownloads != null)
                    {
                        foreach (var item in mDownloads)
                        {
                            _musicDownloads[item.Key] = item.Value;
                        }
                    }
                }
            }

            DiscoverActiveDownloads();
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

        /// <summary>
        /// Запускает процесс фоновой загрузки переданных элементов и возвращает список произошедших ошибок.
        /// </summary>
        /// <param name="items">Список элементов для загрузки.</param>
        public async Task<List<DownloadInitError>> StartDownloadingAsync(IList<IDownloadable> items)
        {
            StorageFolder musicFolder = null;
            StorageFolder videosFolder = null;
            StorageFolder otherFolder = null;
            StorageFolder imagesFolder = null;

            var errors = new List<DownloadInitError>();

            for (int i = 0; i < items.Count; i++)
            {
                if (_downloads.Count == INIT_DOWNLOADS_LIST_CAPACITY)
                {
                    errors.Add(new DownloadInitError(DownloadInitErrorType.MaxDownloadsExceeded, items[i]));
                    continue;
                }

                var item = items[i];
                StorageFolder currentFolder = null;
                switch (item.ContentType)
                {
                    case FileContentType.Music:
                        if (musicFolder == null)
                            musicFolder = await GetFolderFromType(item.ContentType);
                        currentFolder = musicFolder;
                        break;
                    case FileContentType.Video:
                        if (videosFolder == null)
                            videosFolder = await GetFolderFromType(item.ContentType);
                        currentFolder = videosFolder;
                        break;
                    case FileContentType.Image:
                        if (imagesFolder == null)
                            imagesFolder = await GetFolderFromType(item.ContentType);
                        currentFolder = imagesFolder;
                        break;
                    default:
                        if (otherFolder == null)
                            otherFolder = await GetFolderFromType(item.ContentType);
                        currentFolder = otherFolder;
                        break;
                }

                if (currentFolder == null)
                {
                    errors.Add(new DownloadInitError(DownloadInitErrorType.CantCreateFolder, item));
                    continue;
                }

                StorageFile resultFile = await GetFileForDownload(item, currentFolder);
                if (resultFile == null)
                {
                    errors.Add(new DownloadInitError(DownloadInitErrorType.CantCreateFile, item));
                    continue;
                }                

                try
                {
                    var downloader = new BackgroundDownloader() { TransferGroup = _transferGroup };
                    AttachNotifications(downloader, item);

                    var download = downloader.CreateDownload(new Uri(item.Source), resultFile);

                    if (item.ContentType == FileContentType.Music)
                        _musicDownloads[item.FileName] = (VKSaverAudio)item.Metadata;

                    HandleDownloadAsync(download);
                }
                catch (Exception ex)
                {
                    _logService.LogException(ex);
                    await resultFile.DeleteAsync(StorageDeleteOption.PermanentDelete);

                    errors.Add(new DownloadInitError(DownloadInitErrorType.Unknown, item));
                    continue;
                }
            }

            await SaveDownloadsMetadataAsync();
            return errors;
        }

        private void AttachNotifications(BackgroundDownloader downloader, IDownloadable download)
        {
            if (!_settingsService.Get(AppConstants.DOWNLOADS_NOTIFICATIONS_PARAMETER, true))
                return;

            string name = null;
            if (download.ContentType == FileContentType.Music)
                name = ((VKSaverAudio)download.Metadata).Track.Title;
            else
                name = download.FileName;

            var successToast = ToastContentFactory.CreateToastText02();
            successToast.Audio.Content = ToastAudioContent.SMS;
            successToast.TextHeading.Text = _locService["Toast_Downloads_Success_Text"];
            successToast.TextBodyWrap.Text = name;

            var successXml = successToast.GetXml();
            ToastAudioHelper.SetSuccessAudio(successXml);

            var failToast = ToastContentFactory.CreateToastText02();
            failToast.Audio.Content = ToastAudioContent.IM;
            failToast.TextHeading.Text = _locService["Toast_Downloads_Fail_Text"];
            failToast.TextBodyWrap.Text = name;

            var failXml = failToast.GetXml();
            ToastAudioHelper.SetFailAudio(failXml);

            downloader.SuccessToastNotification = new ToastNotification(successXml);
            downloader.FailureToastNotification = new ToastNotification(failXml);
        }

        private async void HandleDownloadAsync(DownloadOperation operation, bool start = true)
        {
            var tokenSource = new CancellationTokenSource();
            var callback = new Progress<DownloadOperation>(OnDownloadProgressChanged);
            _downloads.Add(operation);
            _cts.Add(operation.Guid, tokenSource);

            OnDownloadProgressChanged(operation);

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
                DownloadError?.Invoke(this, new TransferOperationErrorEventArgs(
                    operation.Guid,
                    GetOperationNameFromFile(operation.ResultFile),
                    GetContentTypeFromExtension(operation.ResultFile.FileType), 
                    ex));
            }

            string fileName = operation.ResultFile.Name.Split(new char[] { '.' })[0];

            try
            {
                if (operation.Progress.Status == BackgroundTransferStatus.Completed)
                {
                    var type = GetContentTypeFromExtension(operation.ResultFile.FileType);
                    if (type == FileContentType.Music)
                    {
                        VKSaverAudio metadata = null;
                        _musicDownloads.TryGetValue(fileName, out metadata);

                        await _musicCacheService.PostprocessAudioAsync((StorageFile)operation.ResultFile, metadata);
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
            }

            OnDownloadProgressChanged(operation);

            _cts.Remove(operation.Guid);
            _downloads.Remove(operation);
            _musicDownloads.Remove(fileName);

            if (_downloads.Count == 0)
            {
                DownloadsCompleted?.Invoke(this, EventArgs.Empty);
                await GetMetadataFileAsync(true);
            }
        }

        private async Task SaveDownloadsMetadataAsync()
        {
            string json = JsonConvert.SerializeObject(_musicDownloads);
            var stream = await GetMetadataFileAsync(true);
            var writter = new StreamWriter(stream);

            writter.Write(json);
            writter.Dispose();
        }

        /// <summary>
        /// Вызывается при изменении прогресса какой-либо загрузки.
        /// </summary>
        private void OnDownloadProgressChanged(DownloadOperation e)
        {
            ProgressChanged?.Invoke(this, new TransferItem
            {
                OperationGuid = e.Guid,
                Name = GetOperationNameFromFile(e.ResultFile),
                ContentType = GetContentTypeFromExtension(e.ResultFile.FileType),
                Status = e.Progress.Status,
                TotalSize = FileSize.FromBytes(e.Progress.TotalBytesToReceive),
                ProcessedSize = FileSize.FromBytes(e.Progress.BytesReceived)
            });
        }
        
        /// <summary>
        /// Возвращает название операции загрузки из файла.
        /// </summary>
        /// <param name="file">Результирующий файл.</param>
        private string GetOperationNameFromFile(IStorageFile file)
        {
            string fileName = file.Name.Split(new char[] { '.' })[0];
            var type = GetContentTypeFromExtension(file.FileType);
            switch (type)
            {
                case FileContentType.Music:
                    VKSaverAudio metadata = null;
                    _musicDownloads.TryGetValue(fileName, out metadata);

                    if (metadata != null)
                        return metadata.Track.Title;
                    break;
            }

            return fileName;
        }

        /// <summary>
        /// Возвращает файл, в который будет выполняться загрузка.
        /// </summary>
        /// <param name="item">Элемент, для которого требуется создать файл.</param>
        /// <param name="folder">Папка, в которой требуется создать файл.</param>
        private static async Task<StorageFile> GetFileForDownload(IDownloadable item, StorageFolder folder)
        {
            try
            {
                return await folder.CreateFileAsync(GetSafeFileName(item.FileName) + item.Extension, 
                    CreationCollisionOption.GenerateUniqueName);
            }
            catch (Exception) { return null; }
        }

        private async Task<Stream> GetMetadataFileAsync(bool clear)
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    DOWNLOADS_METADATA_FILE_NAME, 
                    clear ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.OpenIfExists);
                var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
                return stream.AsStream();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
                return null;
            }
        }

        private bool _isRunning;
        private bool _downloadsAttached;

        private readonly BackgroundTransferGroup _transferGroup;
        private readonly List<DownloadOperation> _downloads;
        private readonly Dictionary<Guid, CancellationTokenSource> _cts;
        private readonly Dictionary<string, VKSaverAudio> _musicDownloads;

        private readonly IMusicCacheService _musicCacheService;
        private readonly ISettingsService _settingsService;
        private readonly ILogService _logService;
        private readonly ILocService _locService;

        private readonly object _lockObject = new object();

        private const string DOWNLOADS_TRANSFER_GROUP_NAME = "VKSaverDownloader";
        private const string DOWNLOADS_FOLDER_NAME = "VKSaver";
        private const string DOWNLOADS_OTHER_FOLDER_NAME = "VKSaver Other";
        private const string MUSIC_DOWNLOADS_PARAMETER = "MusicDownloads";
        private const string DOWNLOADS_METADATA_FILE_NAME = "DownloadsMetadata.txt";
        private const int INIT_DOWNLOADS_LIST_CAPACITY = 60;        
    }
}
