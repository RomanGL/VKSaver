using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Services.VksmExtraction;
using Windows.Storage;
using Windows.Storage.Search;
using static VKSaver.Core.Services.Common.DownloadsExtensions;

namespace VKSaver.Core.Services
{
    public sealed class VksmExtractionService : IVksmExtractionService
    {
        public event TypedEventHandler<IVksmExtractionService, SearchingProgressChangedEventArgs> SearchingProgressChanged;
        public event TypedEventHandler<IVksmExtractionService, ExtractingProgressChangedEventArgs> ExtractingProgressChanged;
        public event TypedEventHandler<IVksmExtractionService, EventArgs> ExtractionCompleted;

        public VksmExtractionService(ILogService logService)
        {
            _logService = logService;
        }

        public Task ExtractAsync()
        {
            return StartSearchingFilesAsync();
        }

        private async Task StartSearchingFilesAsync()
        {
            _totalFiles = 0;
            _currentFile = 0;
            OnSearchingProgressChanged();

            try
            {
                await RecursiveScanFolderAsync(KnownFolders.MusicLibrary);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
            }

            OnExtractionCompleted();
        }

        private async Task RecursiveScanFolderAsync(StorageFolder folder)
        {
            var folders = await folder.GetFoldersAsync();
            foreach (var childFolder in folders)
            {
                await RecursiveScanFolderAsync(childFolder);
            }

            var files = await folder.GetFilesAsync(CommonFileQuery.DefaultQuery);
            _totalFiles += files.Count;

            foreach (var file in files)
            {
                _currentFile++;
                OnSearchingProgressChanged();
                await ProcessFileAsync(file, folder);
            }
        }

        private async Task ProcessFileAsync(StorageFile file, StorageFolder folder)
        {
            if (file.FileType == TEMP_FILES_EXTENSION)
            {
                try
                {
                    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
                catch (Exception ex)
                {
                    _logService.LogException(ex);
                }

                return;
            }
            else if (file.FileType != MusicCacheService.FILES_EXTENSION)
                return;

            StorageFile newFile = null;
            try
            {
                VKSaverAudio metadata = null;
                using (var audioFile = new VKSaverAudioFile(file))
                {
                    metadata = await audioFile.GetMetadataAsync();
                    using (var sourceStream = await audioFile.GetContentStreamAsync())
                    {
                        newFile = await folder.CreateFileAsync(
                            Path.GetFileNameWithoutExtension(file.Name) + TEMP_FILES_EXTENSION,
                            CreationCollisionOption.ReplaceExisting);

                        using (var fileStream = await newFile.OpenStreamForWriteAsync())
                        {
                            await CopyStreamAsync(sourceStream, fileStream);
                        }
                    }
                }

                await newFile.RenameAsync(GetSafeFileName(metadata.Track.Title) + MP3_FILES_EXTENSION,
                        NameCollisionOption.GenerateUniqueName);
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
                await newFile?.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }

        private Task CopyStreamAsync(Stream sourceStream, Stream destinationStream)
        {
            return Task.Run(() =>
            {
                var buffer = new byte[4096];

                long completedBytes = 0;
                long totalBytes = sourceStream.Length;

                do
                {
                    var readedBytes = sourceStream.Read(buffer, 0, BUFFER_SIZE);
                    destinationStream.Write(buffer, 0, readedBytes);

                    completedBytes += readedBytes;
                    OnExtractingProgressChanged(totalBytes, completedBytes);
                } while (completedBytes < totalBytes);

                destinationStream.Flush();
            });
        }

        private void OnSearchingProgressChanged()
        {
            SearchingProgressChanged?.Invoke(this, new SearchingProgressChangedEventArgs(_totalFiles, _currentFile));
        }

        private void OnExtractingProgressChanged(long totalBytes, long completedBytes)
        {
            ExtractingProgressChanged?.Invoke(this, new ExtractingProgressChangedEventArgs(totalBytes, completedBytes));
        }

        private void OnExtractionCompleted()
        {
            ExtractionCompleted?.Invoke(this, EventArgs.Empty);
        }

        private int _totalFiles;
        private int _currentFile;

        private readonly ILogService _logService;

        public const string TEMP_FILES_EXTENSION = ".vksmtmp";
        public const string MP3_FILES_EXTENSION = ".mp3";
        private const int BUFFER_SIZE = 4096;
    }
}
