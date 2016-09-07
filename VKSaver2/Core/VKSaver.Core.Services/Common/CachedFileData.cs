using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
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

        public async Task<Stream> GetStream()
        {
            await OpenZip();
            var content = _zip.GetEntry("content.vks");
            var stream = _zip.GetInputStream(content);
            return new MusicEncryptedStream(stream);
        }

        public async Task<VKSaverAudio> GetAudioInfo()
        {
            await OpenZip();
            var metadata = _zip.GetEntry("metadata.vks");
            var stream = _zip.GetInputStream(metadata);

            using (var streamReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var serializer = new JsonSerializer();
                var info = serializer.Deserialize<VKSaverAudio>(jsonReader);
                return info;
            }
        }

        public void Dispose()
        {
            _zip?.Close();

            _fileStream = null;
            _zip = null;

            GC.SuppressFinalize(this);
        }

        private Task OpenZip()
        {
            return Task.Run(async () =>
            {
                if (_isZipOpen)
                    return;

                _isZipOpen = true;
                _fileStream = (await _managedFile.OpenReadAsync()).AsStream();
                _zip = new ZipFile(_fileStream);
            });
        }

        private bool _isZipOpen;
        private ZipFile _zip;
        private Stream _fileStream;
        private VKSaverAudio _metadata;

        private readonly StorageFile _managedFile;
    }
}
