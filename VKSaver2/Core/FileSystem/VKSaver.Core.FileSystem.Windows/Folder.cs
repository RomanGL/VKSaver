using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;

namespace VKSaver.Core.FileSystem
{
    public sealed class Folder : IFolder
    {
        private readonly StorageFolder _folder;

        public Folder(StorageFolder folder)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));

            _folder = folder;
        }

        public string DisplayName => _folder.DisplayName;
        public string Path => _folder.Path;

        public async Task<IFile> CreateFileAsync(string desiredName)
        {
            var file = await _folder.CreateFileAsync(desiredName);
            return new File(file);
        }

        public async Task<IFile> CreateFileAsync(string desiredName, CreationCollisionOption options)
        {
            var winOptions = (Windows.Storage.CreationCollisionOption)(int)options;
            var file = await _folder.CreateFileAsync(desiredName, winOptions);
            return new File(file);
        }

        public Task DeleteAsync(bool isPermanent = true)
        {
            return _folder.DeleteAsync(isPermanent ?
                StorageDeleteOption.PermanentDelete :
                StorageDeleteOption.Default)
                .AsTask();
        }

        public async Task<IFile> GetFileAsync(string name)
        {
            var file = await _folder.GetFileAsync(name);
            return new File(file);
        }

        public async Task<IReadOnlyList<IFile>> GetFilesAsync()
        {
            var files = await _folder.GetFilesAsync();
            var fsFiles = files.Select(f => new File(f)).ToList();
            return new ReadOnlyCollection<File>(fsFiles);
        }

        public async Task<IReadOnlyList<IFile>> GetFilesAsync(uint startIndex, uint maxItemsToRetrieve)
        {
            var files = await _folder.GetFilesAsync(CommonFileQuery.DefaultQuery, startIndex, maxItemsToRetrieve);
            var fsFiles = files.Select(f => new File(f)).ToList();
            return new ReadOnlyCollection<File>(fsFiles);
        }

        public async Task<IFolder> GetFolderAsync(string name)
        {
            var folder = await _folder.GetFolderAsync(name);
            return new Folder(folder);
        }

        public async Task<IReadOnlyList<IFolder>> GetFoldersAsync()
        {
            var folders = await _folder.GetFoldersAsync(CommonFolderQuery.DefaultQuery);
            var fsFolders = folders.Select(f => new Folder(f)).ToList();
            return new ReadOnlyCollection<Folder>(fsFolders);
        }
    }
}
