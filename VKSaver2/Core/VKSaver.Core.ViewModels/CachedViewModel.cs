using Microsoft.Practices.Prism.StoreApps;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Collections;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class CachedViewModel : ViewModelBase
    {
        public CachedViewModel(IMusicCacheService musicCacheService)
        {
            _musicCacheService = musicCacheService;
        }

        public PaginatedCollection<CachedTrack> CachedTracks { get; set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                CachedTracks = new PaginatedCollection<CachedTrack>(LoadMoreTracks);
            }

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async Task<IEnumerable<CachedTrack>> LoadMoreTracks(uint page)
        {
            var files = await _musicCacheService.GetCachedFiles();
            var tracks = new List<CachedTrack>();

            foreach (var file in files)
            {
                var metadata = await file.GetMetadataAsync();
                var track = new CachedTrack
                {
                    File = file,
                    Title = metadata.Track.Title,
                    Artist = metadata.Track.Artist,
                    Source = $"vks-token:{file.File.Path}",
                    Duration = TimeSpan.FromTicks(metadata.Track.Duration),
                    VKInfo = metadata.VK
                };
                tracks.Add(track);
            }

            return tracks;
        }

        private readonly IMusicCacheService _musicCacheService;
    }
}
