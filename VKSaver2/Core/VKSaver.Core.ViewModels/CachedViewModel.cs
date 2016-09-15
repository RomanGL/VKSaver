using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
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
using System.Collections;
using Windows.Storage.AccessCache;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class CachedViewModel : AudioViewModel<CachedTrack>
    {
        public CachedViewModel(
            IPlayerService playerService,
            ILocService locService,
            INavigationService navigationService,
            IAppLoaderService appLoaderService,
            IMusicCacheService musicCacheService)
            : base(playerService, locService, navigationService, appLoaderService)
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

        protected override IList<CachedTrack> GetAudiosList()
        {
            return CachedTracks;
        }

        protected override IList GetSelectionList()
        {
            return CachedTracks;
        }

        protected override IPlayerTrack ConvertToPlayerTrack(CachedTrack track)
        {
            return track;
        }

        protected override void PrepareTracksBeforePlay(IEnumerable<CachedTrack> tracks)
        {
            StorageApplicationPermissions.FutureAccessList.Clear();

            foreach (var track in tracks)
            {
                string token = StorageApplicationPermissions.FutureAccessList.Add(track.File.File);
                track.Source = $"vks-token:{token}";
            }

            base.PrepareTracksBeforePlay(tracks);
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
                    Duration = TimeSpan.FromTicks(metadata.Track.Duration),
                    VKInfo = metadata.VK
                };
                tracks.Add(track);
            }

            if (tracks.Count > 0)
                SetDefaultMode();

            return tracks;
        }

        private readonly IMusicCacheService _musicCacheService;
    }
}
