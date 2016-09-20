using System;
using System.IO;
using System.Threading.Tasks;
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

        public Stream GetDataStream()
        {
            return GetDataStreamAsync().GetAwaiter().GetResult();
        }

        public async Task<Stream> GetDataStreamAsync()
        {
            return await _file.OpenStreamForReadAsync();
        }

        private readonly StorageFile _file;
    }
}
