using System;
using System.IO;

namespace VKSaver.Core.Models.Common
{
    internal sealed class AudioEncryptedStream : Stream
    {
        public AudioEncryptedStream(Stream contentStream)
        {
            if (contentStream == null)
                throw new ArgumentNullException("contentStream");
            _contentStream = contentStream;
        }

        public AudioEncryptedStream(Stream contentStream, AudioEncryptionMethod encryption)
            : this(contentStream)
        {
            _encryption = encryption;
        }

        public override bool CanRead { get { return _contentStream.CanRead; } }

        public override bool CanSeek { get { return _contentStream.CanSeek; } }

        public override bool CanWrite { get { return _contentStream.CanWrite; } }

        public override long Length { get { return _contentStream.Length; } }

        public override long Position
        {
            get { return _contentStream.Position; }
            set { _contentStream.Position = value; }
        }

        public override void Flush()
        {
            _contentStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _contentStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _contentStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {            
            return _contentStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _contentStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            _contentStream.Dispose();
            base.Dispose(disposing);
        }

        private readonly Stream _contentStream;
        private readonly AudioEncryptionMethod _encryption;
    }
}
