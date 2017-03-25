using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace VKSaver.Core.FileSystem
{
    public sealed class Folder : IFolder
    {
        private readonly StorageFolder _folder;

        public Folder(StorageFolder folder)
        {
            _folder = folder;
        }

        public string DisplayName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Path
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Task<IFile> CreateFileAsync(string desiredName)
        {
            throw new NotImplementedException();
        }

        public Task<IFile> CreateFileAsync(string desiredName, CreationCollisionOption options)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(bool isPermanent)
        {
            throw new NotImplementedException();
        }

        public Task<IFile> GetFileAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<IFile>> GetFilesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<IFile>> GetFilesAsync(uint startIndex, uint maxItemsToRetrieve)
        {
            throw new NotImplementedException();
        }

        public Task<IFolder> GetFolderAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<IFolder>> GetFoldersAsync()
        {
            throw new NotImplementedException();
        }
    }
}
