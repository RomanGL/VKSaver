using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Services.Interfaces;
using Windows.Storage;

namespace VKSaver.Core.Services
{
    public sealed class PlayerPlaylistService : IPlayerPlaylistService
    {
        public PlayerPlaylistService(ILogService logService)
        {
            _logService = logService;
        }

        public Task<IEnumerable<IPlayerTrack>> ReadPlaylist()
        {
            return GetPlaylistFromFile(NORMAL_PLAYLIST_NAME);
        }

        public Task<IEnumerable<IPlayerTrack>> ReadShuffledPlaylist()
        {
            return GetPlaylistFromFile(SHUFFLED_PLAYLIST_NAME);
        }

        public Task<bool> WritePlaylist(IEnumerable<IPlayerTrack> tracks)
        {
            return WritePlaylistToFile(NORMAL_PLAYLIST_NAME, tracks);
        }

        public Task<bool> WriteShuffledPlaylist(IEnumerable<IPlayerTrack> tracks)
        {
            return WritePlaylistToFile(SHUFFLED_PLAYLIST_NAME, tracks);
        }

        private async Task<IEnumerable<IPlayerTrack>> GetPlaylistFromFile(string fileName)
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);

                using (var stream = await file.OpenStreamForReadAsync())
                using (var strReader = new StreamReader(stream))
                using (var reader = new JsonTextReader(strReader))
                {
                    var serializer = new JsonSerializer();
                    var tracks = serializer.Deserialize<List<PlayerTrack>>(reader);
                    return tracks;
                }
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
                return null;
            }
        }

        private async Task<bool> WritePlaylistToFile(string fileName, IEnumerable<IPlayerTrack> tracks)
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

                using (var stream = await file.OpenStreamForWriteAsync())
                using (var strWritter = new StreamWriter(stream))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(strWritter, tracks);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
                return false;
            }
        }

        private readonly ILogService _logService;

        private const string NORMAL_PLAYLIST_NAME = "Normal.vkspl";
        private const string SHUFFLED_PLAYLIST_NAME = "Shuffled.vkspl";
    }
}
