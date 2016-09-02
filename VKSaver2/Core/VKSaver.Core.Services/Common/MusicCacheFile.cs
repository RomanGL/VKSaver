using System;
using System.IO;
using static TagLib.File;

namespace VKSaver.Core.Services.Common
{
    public sealed class MusicCacheFile : IFileAbstraction
    {
        public MusicCacheFile(string fileName, Stream readStream)
        {
            Name = fileName;
            ReadStream = readStream;
        }

        public string Name { get; private set; }

        public Stream ReadStream { get; private set; }

        public Stream WriteStream
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void CloseStream(Stream stream)
        {
            stream.Dispose();
        }
    }
}
