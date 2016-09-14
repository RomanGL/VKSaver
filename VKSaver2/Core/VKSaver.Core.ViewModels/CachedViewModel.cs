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
            if (viewModelState.Count > 0)
            {
                _offset = (uint)viewModelState[nameof(_offset)];
                CachedTracks = JsonConvert.DeserializeObject<PaginatedCollection<CachedTrack>>(
                    viewModelState[nameof(CachedTracks)].ToString());

                CachedTracks.LoadMoreItems = LoadMoreTracks;
            }
            else
            {
                _offset = 0;
                CachedTracks = new PaginatedCollection<CachedTrack>(LoadMoreTracks);
            }

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                viewModelState[nameof(CachedTracks)] = JsonConvert.SerializeObject(CachedTracks);
                viewModelState[nameof(_offset)] = _offset;
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async Task<IEnumerable<CachedTrack>> LoadMoreTracks(uint page)
        {
            var files = await _musicCacheService.GetCachedFiles(20, _offset);
            var tracks = new List<CachedTrack>(20);
            _offset += 20;

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

        private uint _offset;

        private readonly IMusicCacheService _musicCacheService;
    }
}
