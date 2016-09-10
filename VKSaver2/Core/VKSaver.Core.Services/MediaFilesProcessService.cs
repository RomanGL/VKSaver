using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Interfaces;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace VKSaver.Core.Services
{
    public sealed class MediaFilesProcessService : IMediaFilesProcessService
    {
        public MediaFilesProcessService(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        public Task ProcessFiles(IEnumerable<IStorageItem> filesToOpen)
        {
            return Task.Run(async () => await ProcessFilesInternal(filesToOpen));
        }

        private async Task ProcessFilesInternal(IEnumerable<IStorageItem> filesToOpen)
        {
            StorageApplicationPermissions.FutureAccessList.Clear();

            var tracks = new List<PlayerTrack>(filesToOpen.Count());

            foreach (StorageFile item in filesToOpen)
            {
                var cachedData = new CachedFileData(item);
                var info = await cachedData.GetAudioInfo();

                var track = new PlayerTrack
                {
                    Title = info.Track.Title,
                    Artist = info.Track.Artist,
                    VKInfo = info.VK
                };

                string token = StorageApplicationPermissions.FutureAccessList.Add(item);
                track.Source = $"vks-token:{token}";

                tracks.Add(track);
            }

            await _playerService.PlayNewTracks(tracks, 0);
        }

        private readonly IPlayerService _playerService;
    }
}
