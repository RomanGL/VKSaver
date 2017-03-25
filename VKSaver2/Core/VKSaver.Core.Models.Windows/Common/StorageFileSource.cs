using System;
using System.IO;
using System.Threading.Tasks;
using VKSaver.Core.FileSystem;
using Windows.Storage;

namespace VKSaver.Core.Models.Common
{
    public sealed class StorageFileSource : IDataSource, IDisposable
    {
        public StorageFileSource(StorageFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            _file = file;
        }

        public void Dispose() { }

        public IFile GetFile()
        {
            // TODO: IFile
            throw new NotImplementedException("IFile");
            //return _file;
        }

        public Stream GetDataStream()
        {
            return GetDataStreamAsync().GetAwaiter().GetResult();
        }

        public async Task<Stream> GetDataStreamAsync()
        {
            return await _file.OpenStreamForReadAsync();
        }

        public string GetContentType()
        {
            if (String.IsNullOrEmpty(_file.ContentType))
                return "application/vksaver";
            return _file.ContentType;
        }

        private readonly StorageFile _file;
    }
}
