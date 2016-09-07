using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Interfaces;
using Windows.Storage;

namespace VKSaver.Core.Services
{
    public sealed class MediaFilesProcessService : IMediaFilesProcessService
    {
        public MediaFilesProcessService(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        public Task ProcessFiles(IList<IStorageItem> filesToOpen)
        {
            return Task.Run(async () => await ProcessFilesInternal(filesToOpen));
        }

        private async Task ProcessFilesInternal(IList<IStorageItem> filesToOpen)
        {
            var tracks = new List<PlayerTrack>(filesToOpen.Count);

            foreach (var item in filesToOpen)
            {
                var openedFile = await StorageFile.GetFileFromPathAsync(item.Path);
                var cachedData = new CachedFileData(openedFile);
                var info = await cachedData.GetAudioInfo();

                tracks.Add(new PlayerTrack
                {
                    Title = info.Track.Title,
                    Artist = info.Track.Artist,
                    VKInfo = info.VK
                });
            }

            await _playerService.PlayNewTracks(tracks, 0);
        }

        private readonly IPlayerService _playerService;
    }
}
