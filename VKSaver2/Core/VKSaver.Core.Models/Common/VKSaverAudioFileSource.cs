using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace VKSaver.Core.Models.Common
{
    public sealed class VKSaverAudioFileSource : IDataSource, IDisposable
    {
        public VKSaverAudioFileSource(VKSaverAudioFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            _file = file;
        }

        public void Dispose()
        {
            _file.Dispose();
        }

        public StorageFile GetFile()
        {
            return _file.File;
        }

        public Stream GetDataStream()
        {
            return _file.GetContentStreamAsync().GetAwaiter().GetResult();
        }

        public Task<Stream> GetDataStreamAsync()
        {
            return _file.GetContentStreamAsync();
        }

        public string GetContentType()
        {
            return "audio/mpeg";
        }

        private readonly VKSaverAudioFile _file;
    }
}
