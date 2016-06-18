using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Transfer;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Web;

namespace VKSaver.Core.Services
{
    /// <summary>
    /// Представляет сервис для рабоы с загрузками.
    /// </summary>
    public sealed class DownloadsService : IDownloadsService
    {        
        /// <summary>
        /// Происходит при изменении прогресса загрузки.
        /// </summary>
        public event EventHandler<DownloadItem> ProgressChanged;
        /// <summary>
        /// Происходит при возникновении серьезной ошибки загрузки.
        /// </summary>
        public event EventHandler<DownloadOperationErrorEventArgs> DownloadError;

        /// <summary>
        /// Статический конструктор.
        /// </summary>
        public DownloadsService()
        {
            _transferGroup = BackgroundTransferGroup.CreateGroup(DOWNLOAD_TRASNFER_GROUP_NAME);
            _transferGroup.TransferBehavior = BackgroundTransferBehavior.Serialized;
            _downloads = new List<DownloadOperation>(INIT_DOWNLOADS_LIST_CAPACITY);
            _cts = new Dictionary<Guid, CancellationTokenSource>(INIT_DOWNLOADS_LIST_CAPACITY);
        }

        /// <summary>
        /// Выполняется ли в данный момент загрузка текущих загрузок.
        /// </summary>
        public bool IsLoading { get; private set; }

        /// <summary>
        /// Возвращает массив со всеми выполняющимися загрузками.
        /// </summary>
        public DownloadItem[] GetAllDownloads()
        {
            if (_downloads.Count == 0) return null;
            return _downloads.Select(e => new DownloadItem
            {
                OperationGuid = e.Guid,
                Name = GetOperationNameFromFileName(e.ResultFile.Name),
                ContentType = GetContentTypeFromExtension(e.ResultFile.FileType),
                Status = e.Progress.Status,
                TotalSize = FileSize.FromBytes(e.Progress.TotalBytesToReceive),
                DownloadedSize = FileSize.FromBytes(e.Progress.BytesReceived)
            }).ToArray();
        }

        /// <summary>
        /// Найти все фоновые загрузки и обработать их.
        /// </summary>
        public async void DiscoverActiveDownloadsAsync()
        {
            IsLoading = true;
            IReadOnlyList<DownloadOperation> downloads = null;

            try { downloads = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(_transferGroup); }
            catch (Exception ex) { WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult); }

            if (downloads != null && downloads.Count > 0)
                for (int i = 0; i < downloads.Count; i++)
                    HandleDownloadAsync(downloads[i], false);
            IsLoading = false;
        }

        /// <summary>
        /// Запускает процесс фоновой загрузки переданных элементов и возвращает список произошедших ошибок.
        /// </summary>
        /// <param name="items">Список элементов для загрузки.</param>
        public async Task<List<DownloadInitError>> StartDownloadingAsync(IList<IDownloadable> items)
        {
            StorageFolder musicFolder = null;
            StorageFolder videosFolder = null;
            StorageFolder documentsFolder = null;
            StorageFolder imagesFolder = null;

            var errors = new List<DownloadInitError>();

            for (int i = 0; i < items.Count; i++)
            {
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
                        if (documentsFolder == null)
                            documentsFolder = await GetFolderFromType(item.ContentType);
                        currentFolder = documentsFolder;
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
                    var download = downloader.CreateDownload(new Uri(item.Source), resultFile);
                    HandleDownloadAsync(download);
                }
                catch (Exception)
                {
                    errors.Add(new DownloadInitError(DownloadInitErrorType.Unknown, item));
                    continue;
                }
            }

            return errors;
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
                if (start) await operation.StartAsync().AsTask(tokenSource.Token, callback);
                else await operation.AttachAsync().AsTask(tokenSource.Token, callback);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                DownloadError?.Invoke(this, new DownloadOperationErrorEventArgs(
                    operation.Guid, 
                    GetOperationNameFromFileName(operation.ResultFile.Name),
                    GetContentTypeFromExtension(operation.ResultFile.FileType), 
                    ex));
            }

            OnDownloadProgressChanged(operation);
            if (operation.Progress.Status == BackgroundTransferStatus.Canceled ||
                operation.Progress.Status == BackgroundTransferStatus.Error)
                await operation.ResultFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            
            _cts.Remove(operation.Guid);
            _downloads.Remove(operation);
        }

        /// <summary>
        /// Вызывается при изменении прогресса какой-либо загрузки.
        /// </summary>
        private void OnDownloadProgressChanged(DownloadOperation e)
        {
            ProgressChanged?.Invoke(this, new DownloadItem
            {
                OperationGuid = e.Guid,
                Name = GetOperationNameFromFileName(e.ResultFile.Name),
                ContentType = GetContentTypeFromExtension(e.ResultFile.FileType),
                Status = e.Progress.Status,
                TotalSize = FileSize.FromBytes(e.Progress.TotalBytesToReceive),
                DownloadedSize = FileSize.FromBytes(e.Progress.BytesReceived)
            });
        }
        
        /// <summary>
        /// Возвращает название операции загрузки из имени файла.
        /// </summary>
        /// <param name="fileName">Имя результирующего файла.</param>
        private static string GetOperationNameFromFileName(string fileName)
        {
            return fileName.Split(new char[] { '.' })[0];
        }

        /// <summary>
        /// Возвращает тип содержимого по расширению файла.
        /// </summary>
        /// <param name="fileExtension">Расширение файла, например ".mp3".</param>
        private static FileContentType GetContentTypeFromExtension(string fileExtension)
        {
            switch (fileExtension)
            {
                case ".mp3": return FileContentType.Music;
                case ".wma": return FileContentType.Music;
                case ".wav": return FileContentType.Music;
                case ".aac": return FileContentType.Music;                
                case ".m4a": return FileContentType.Music;
                case ".flac": return FileContentType.Music;

                case ".mp4": return FileContentType.Video;
                case ".mkv": return FileContentType.Video;
                case ".avi": return FileContentType.Video;
                case ".3gp": return FileContentType.Video;

                case ".jpg": return FileContentType.Image;                
                case ".jpe": return FileContentType.Image;
                case ".jpeg": return FileContentType.Image;
                case ".bmp": return FileContentType.Image;
                case ".png": return FileContentType.Image;
                case ".gif": return FileContentType.Image;

                default: return FileContentType.Other;
            }
        }

        /// <summary>
        /// Возвращает папку для загрузки содержимого переданного типа.
        /// </summary>
        /// <param name="type">Тип содержимого.</param>
        private static async Task<StorageFolder> GetFolderFromType(FileContentType type)
        {
            try
            {
                StorageFolder rootFolder = null;

                switch (type)
                {
                    case FileContentType.Music:
                        rootFolder = KnownFolders.MusicLibrary;
                        break;
                    case FileContentType.Video:
                        rootFolder = KnownFolders.VideosLibrary;
                        break;
                    case FileContentType.Image:
                        rootFolder = KnownFolders.SavedPictures;
                        break;
                    default:
                        return await KnownFolders.PicturesLibrary.CreateFolderAsync(DOWNLOADS_OTHER_FOLDER_NAME,
                            CreationCollisionOption.OpenIfExists);
                }
                                
                return await rootFolder.CreateFolderAsync(DOWNLOADS_FOLDER_NAME, 
                    CreationCollisionOption.OpenIfExists);
            }
            catch (Exception)
            {
                return null;
            }
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

        /// <summary>
        /// Возвращает допустимое имя файла.
        /// </summary>
        /// <param name="fileName">Исходное имя файла, которое требуется обработать.</param>
        private static string GetSafeFileName(string fileName)
        {
            if (fileName.Length > 50)
                fileName = fileName.Remove(50);

            var regex = new Regex("[?*.<>:|&/\"]");
            return regex.Replace(fileName, String.Empty);
        }        

        private readonly BackgroundTransferGroup _transferGroup;
        private readonly List<DownloadOperation> _downloads;
        private readonly Dictionary<Guid, CancellationTokenSource> _cts;

        private const string DOWNLOAD_TRASNFER_GROUP_NAME = "VKSaverDownloader";
        private const string DOWNLOADS_FOLDER_NAME = "VKSaver";
        private const string DOWNLOADS_OTHER_FOLDER_NAME = "VKSaver Other";
        private const int INIT_DOWNLOADS_LIST_CAPACITY = 50;
    }
}
