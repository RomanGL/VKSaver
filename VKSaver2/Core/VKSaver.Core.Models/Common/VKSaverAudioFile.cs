using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace VKSaver.Core.Models.Common
{
    /// <summary>
    /// Представляет файл аудиозаписи VKSaver.
    /// </summary>
    public sealed class VKSaverAudioFile : IVKSaverFile<VKSaverAudio>, IDisposable
    {       
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="VKSaverAudioFile"/>.
        /// </summary>
        /// <param name="file">Файл с данными.</param>
        /// <exception cref="ArgumentNullException"/>
        public VKSaverAudioFile(StorageFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            _file = file;
        }
         
        public StorageFile File { get { return _file; } }

        public async Task<Stream> GetContentStreamAsync()
        {
            await ReadZipFileAsync();

            var contentEntry = _zip.GetEntry(CONTENT_ENTRY_NAME);
            var stream = _zip.GetInputStream(contentEntry);

            var metadata = GetMetadataInternal();

            return new AudioEncryptedStream(stream, metadata.Track.Encryption);
        }

        public async Task<VKSaverAudio> GetMetadataAsync()
        {
            await ReadZipFileAsync();
            return GetMetadataInternal();
        }

        public void Dispose()
        {
            _zip.Close();
            _zip = null;
        }

        private VKSaverAudio GetMetadataInternal()
        {
            var metadata = _zip.GetEntry(METADATA_ENTRY_NAME);
            var stream = _zip.GetInputStream(metadata);

            using (var streamReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var serializer = new JsonSerializer();
                var info = serializer.Deserialize<VKSaverAudio>(jsonReader);
                return info;
            }
        }

        private Task ReadZipFileAsync()
        {
            return Task.Run(() => ReadZipFile());
        }

        private void ReadZipFile()
        {
            lock (_lockObject)
            {
                if (_zip != null)
                    return;

                _fileStream = _file.OpenStreamForReadAsync().GetAwaiter().GetResult();
                _zip = new ZipFile(_fileStream);
                _zip.IsStreamOwner = true;
            }
        }
        
        private ZipFile _zip;
        private Stream _fileStream;

        private readonly StorageFile _file;
        private readonly object _lockObject = new object();

        private const string CONTENT_ENTRY_NAME = "content.vks";
        private const string METADATA_ENTRY_NAME = "metadata.vks"; 
    }
}
