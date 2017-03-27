using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace VKSaver.Core.FileSystem
{
    public sealed class FileProperties : IFileProperties
    {
        private readonly StorageFile _storageFile;

        internal FileProperties(StorageFile storageFile)
        {
            if (storageFile == null)
                throw new ArgumentNullException(nameof(storageFile));

            _storageFile = storageFile;
        }

        public async Task<MusicProperties> GetMusicPropertiesAsync()
        {
            var props = await _storageFile.Properties.GetMusicPropertiesAsync();
            return new MusicProperties
            {
                Album = props.Album,
                AlbumArtist = props.AlbumArtist,
                Artist = props.Artist,
                Bitrate = props.Bitrate,
                Composers = props.Composers,
                Conductors = props.Conductors,
                Duration = props.Duration,
                Genre = props.Genre,
                Producers = props.Producers,
                Publisher = props.Publisher,
                Rating = props.Rating,
                Subtitle = props.Subtitle,
                Title = props.Title,
                TrackNumber = props.TrackNumber,
                Writers = props.Writers,
                Year = props.Year
            };
        }

        public Task<IDictionary<string, object>> RetrievePropertiesAsync(IEnumerable<string> propertiesToRetrieve)
        {
            return _storageFile.Properties.RetrievePropertiesAsync(propertiesToRetrieve).AsTask();
        }
    }
}
