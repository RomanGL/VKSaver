using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace VKSaver.Core.Services.Common
{
    public sealed class CachedFileData : IDisposable
    {
        public CachedFileData(StorageFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            _managedFile = file;
        }

        public async Task<IRandomAccessStream> GetStream()
        {
            if (_isOpen)
                return _contentStream;

            _isOpen = true;

            _fileStream = (await _managedFile.OpenReadAsync()).AsStream();
            _zip = new ZipFile(_fileStream);

            var content = _zip.GetEntry("content.vks");
            _contentStream = _zip.GetInputStream(content).AsRandomAccessStream();

            return _contentStream;
        }

        public void Dispose()
        {
            _contentStream?.Dispose();
            _zip?.Close();

            _fileStream = null;
            _zip = null;
            _contentStream = null;

            GC.SuppressFinalize(this);
        }

        private bool _isOpen;
        private ZipFile _zip;
        private Stream _fileStream;
        private IRandomAccessStream _contentStream;

        private readonly StorageFile _managedFile;
    }
}
