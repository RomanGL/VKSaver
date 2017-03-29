using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace VKSaver.Core.FileSystem
{
    public sealed class File : IWindowsFile, IFile, IDisposable
    {
        private readonly StorageFile _storageFile;
        private readonly IFileProperties _fileProperties;

        public File(StorageFile storageFile)
        {
            if (storageFile == null)
                throw new ArgumentNullException(nameof(storageFile));

            _storageFile = storageFile;
            _fileProperties = new FileProperties(storageFile);
        }

        public StorageFile StorageFile => _storageFile;
        public string DisplayName => _storageFile.DisplayName;
        public string FileType => _storageFile.FileType;
        public string Name => _storageFile.Name;
        public string Path => _storageFile.Path;
        public IFileProperties Properties => _fileProperties;

        public Task DeleteAsync(bool isPermanent = true)
        {
            return _storageFile.DeleteAsync(isPermanent ? 
                StorageDeleteOption.PermanentDelete : 
                StorageDeleteOption.Default)
                .AsTask();
        }

        public async Task<Stream> OpenAsync(FileAccessMode accessMode)
        {
            if (accessMode == FileAccessMode.Read)
            {
                var stream = await _storageFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
                return stream.AsStreamForRead();
            }
            else
            {
                var stream = await _storageFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
                return stream.AsStream();
            }
        }

        public Task RenameAsync(string desiredName, NameCollisionOption option)
        {
            var winOption = (Windows.Storage.NameCollisionOption)(int)option;
            return _storageFile.RenameAsync(desiredName, winOption).AsTask();
        }

        public void Dispose()
        {
        }
    }
}
