﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Services.Transfer;
using VKSaver.Core.Transfer;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Web;
using static VKSaver.Core.Services.Common.DownloadsExtensions;

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

        public Task CancelAll()
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
                if (start)
                    await operation.StartAsync().AsTask(tokenSource.Token, callback);
                else
                    await operation.AttachAsync().AsTask(tokenSource.Token, callback);
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

        private readonly BackgroundTransferGroup _transferGroup;
        private readonly List<DownloadOperation> _downloads;
        private readonly Dictionary<Guid, CancellationTokenSource> _cts;

        private const string DOWNLOAD_TRASNFER_GROUP_NAME = "VKSaverDownloader";
        private const string DOWNLOADS_FOLDER_NAME = "VKSaver";
        private const string DOWNLOADS_OTHER_FOLDER_NAME = "VKSaver Other";
        private const int INIT_DOWNLOADS_LIST_CAPACITY = 50;
    }
}
