#if WINDOWS_UWP || WINDOWS_PHONE_APP
using Windows.Storage.AccessCache;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VKSaver.Core.FileSystem;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class MediaFilesProcessService : IMediaFilesProcessService
    {
        public MediaFilesProcessService(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        public Task ProcessFiles(IEnumerable<IFile> filesToOpen)
        {
            return Task.Run(async () => await ProcessFilesInternal(filesToOpen));
        }

        private async Task ProcessFilesInternal(IEnumerable<IFile> filesToOpen)
        {
#if WINDOWS_UWP || WINDOWS_PHONE_APP
            StorageApplicationPermissions.FutureAccessList.Clear();

            var tracks = new List<PlayerTrack>(filesToOpen.Count());

            foreach (var item in filesToOpen.Cast<IWindowsFile>())
            {
                var cachedData = new VKSaverAudioFile(item);
                var info = await cachedData.GetMetadataAsync();

                var track = new PlayerTrack
                {
                    Title = info.Track.Title,
                    Artist = info.Track.Artist,
                    VKInfo = info.VK
                };

                string token = StorageApplicationPermissions.FutureAccessList.Add(item.StorageFile);
                track.Source = $"vks-token:{token}";

                tracks.Add(track);
            }

            await _playerService.PlayNewTracks(tracks, 0);
#else
            throw new NotImplementedException();
#endif
        }

        private readonly IPlayerService _playerService;
    }
}
