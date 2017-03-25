using System;
using System.IO;
using System.Threading.Tasks;
using VKSaver.Core.FileSystem;

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

        public IFile GetFile()
        {
            // TODO: IFile
            throw new NotImplementedException("IFile");
            //return _file.File;
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
