using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace VKSaver.Core.FileSystem
{
    public sealed class File : IWindowsFile, IFile
    {
        private readonly StorageFile _storageFile;

        public File(StorageFile storageFile)
        {
            _storageFile = storageFile;
        }

        public StorageFile StorageFile
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string DisplayName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string FileType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Name
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

        public IFileProperties Properties
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Task DeleteAsync(bool isPermanent)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}
