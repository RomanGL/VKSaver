#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#else
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#endif

using ModernDev.InTouch;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Collections;
using VKSaver.Core.ViewModels.Common;
using Windows.UI.Xaml.Navigation;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class MainViewModel : BaseMainViewModel
    {
        public MainViewModel(
            InTouch inTouch,
            LastfmClient lfClient,
            IInTouchWrapper inTouchWrapper,
            INavigationService navigationService,
            IPurchaseService purchaseService,
            IPlayerService playerService,
            IDownloadsServiceHelper downloadsServiceHelper,
            IImagesCacheService imagesCacheService,
            IAdsService adsService)
            : base(inTouch,
                  lfClient,
                  inTouchWrapper,
                  navigationService,
                  downloadsServiceHelper,
                  purchaseService,
                  adsService,
                  playerService,
                  imagesCacheService)
        {
            PlayRecommendedTracksCommand = new DelegateCommand<Audio>(OnPlayRecommendedTracksCommand);
            PlayUserTracksCommand = new DelegateCommand<Audio>(OnPlayUserTracksCommand);
        }

        public SimpleStateSupportCollection<Audio> UserTracks { get; private set; }

        public SimpleStateSupportCollection<LastArtist> TopArtistsLF { get; private set; }

        public SimpleStateSupportCollection<Audio> RecommendedTracksVK { get; private set; }


#if WINDOWS_UWP
        public int TotalTracksCount { get; private set; }
#endif

        [DoNotNotify]
        public DelegateCommand<Audio> PlayRecommendedTracksCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<Audio> PlayUserTracksCommand { get; private set; }

#if !WINDOWS_UWP
        public VKAudioWithImage FirstTrack { get; private set; }

        public string HubBackgroundImage { get; private set; }
#endif

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            _backgroundLoaded = false;
            if (viewModelState.Count > 0)
            {
#if !WINDOWS_UWP
                FirstTrack = JsonConvert.DeserializeObject<VKAudioWithImage>(
                    viewModelState[nameof(FirstTrack)].ToString());
#endif

                UserTracks = JsonConvert.DeserializeObject<SimpleStateSupportCollection<Audio>>(
                    viewModelState[nameof(UserTracks)].ToString());
                TopArtistsLF = JsonConvert.DeserializeObject<SimpleStateSupportCollection<LastArtist>>(
                    viewModelState[nameof(TopArtistsLF)].ToString(), _lastImageSetConverter);
                RecommendedTracksVK = JsonConvert.DeserializeObject<SimpleStateSupportCollection<Audio>>(
                    viewModelState[nameof(RecommendedTracksVK)].ToString());

                UserTracks.LoadItems = LoadUserTracks;
                TopArtistsLF.LoadItems = LoadTopArtists;
                RecommendedTracksVK.LoadItems = LoadRecommendedTracks;
            }
            else
            {
                UserTracks = new SimpleStateSupportCollection<Audio>(LoadUserTracks);
                TopArtistsLF = new SimpleStateSupportCollection<LastArtist>(LoadTopArtists);
                RecommendedTracksVK = new SimpleStateSupportCollection<Audio>(LoadRecommendedTracks);
            }

            UserTracks.Load();
            TopArtistsLF.Load();
            RecommendedTracksVK.Load();

#if !WINDOWS_UWP
            TryLoadBackground(FirstTrack);
#endif

            base.OnNavigatedTo(e, viewModelState);

            await Task.Delay(2000);
            _adsService.ActivateAds();
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
#if !WINDOWS_UWP
                viewModelState[nameof(FirstTrack)] = JsonConvert.SerializeObject(FirstTrack);
#endif
                viewModelState[nameof(UserTracks)] = JsonConvert.SerializeObject(UserTracks.ToList());
                viewModelState[nameof(TopArtistsLF)] = JsonConvert.SerializeObject(TopArtistsLF.ToList(), _lastImageSetConverter);
                viewModelState[nameof(RecommendedTracksVK)] = JsonConvert.SerializeObject(RecommendedTracksVK.ToList());
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async Task<IEnumerable<Audio>> LoadUserTracks()
        {
            if (UserTracks.Any())
                return new List<Audio>();

            int trackToLoad = 10;
#if WINDOWS_UWP
            trackToLoad = 10;
#endif

            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.Get(count: trackToLoad));
            if (response.IsError)
                throw new Exception(response.Error.ToString());
            else
            {
                if (response.Data.Items.Count == 0)
                    return new List<Audio>();

#if WINDOWS_UWP
                TotalTracksCount = response.Data.Count;
                var tracks = new List<Audio>(response.Data.Items);
                tracks.Add(new Audio
                {
                    Title = VKSAVER_SEE_ALSO_TEXT
                });
                return tracks;
#else
                FirstTrack = new VKAudioWithImage
                {
                    VKTrack = response.Data.Items[0]
                };

                TryLoadFirstTrackInfo(FirstTrack);
                TryLoadBackground(FirstTrack);
                return response.Data.Items.Skip(1);
#endif
            }
        }

        private async Task<IEnumerable<LastArtist>> LoadTopArtists()
        {
            if (TopArtistsLF.Any())
                return new List<LastArtist>();

            int itemsPerPage = 8;
#if WINDOWS_UWP
            itemsPerPage = 11;
#endif

            var response = await _lfClient.Chart.GetTopArtistsAsync(itemsPerPage: itemsPerPage);
            if (response.Success)
            {
#if WINDOWS_UWP
                var artists = new List<LastArtist>(response.Content);
                artists.Add(new LastArtist
                {
                    Name = VKSAVER_SEE_ALSO_TEXT
                });
                return artists;
#else
                TryLoadTopArtistBackground(response.First());
                return response;
#endif
            }
            else
                throw new Exception("LFChartArtistsResponse isn't valid.");
        }

        private async Task<IEnumerable<Audio>> LoadRecommendedTracks()
        {
            if (RecommendedTracksVK.Any())
                return new List<Audio>();

            int trackToLoad = 10;
#if WINDOWS_UWP
            trackToLoad = 15;
#endif

            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.GetRecommendations(count: trackToLoad));
            if (response.IsError)
                throw new Exception(response.Error.ToString());

#if WINDOWS_UWP
            var tracks = new List<Audio>(response.Data.Items);
            tracks.Add(new Audio
            {
                Title = VKSAVER_SEE_ALSO_TEXT
            });
            return tracks;
#else
            return response.Data.Items;
#endif
        }

#if !WINDOWS_UWP
        private async void TryLoadBackground(VKAudioWithImage track)
        {
            if (_backgroundLoaded)
                return;
            else if (track == null)
            {
                HubBackgroundImage = HUB_BACKGROUND_DEFAULT;
                return;
            }

            _backgroundLoaded = true;

            string imagePath = await _imagesCacheService.GetCachedArtistImage(track.Artist);
            if (imagePath == null)
            {
                HubBackgroundImage = HUB_BACKGROUND_DEFAULT;
                imagePath = await _imagesCacheService.CacheAndGetArtistImage(track.Artist);
            }

            if (imagePath != null)
                HubBackgroundImage = imagePath;
            else
                _backgroundLoaded = false;
        }

        private async void TryLoadTopArtistBackground(LastArtist artist)
        {
            if (_backgroundLoaded)
                return;
            else if (artist == null)
            {
                HubBackgroundImage = HUB_BACKGROUND_DEFAULT;
                return;
            }

            _backgroundLoaded = true;

            string imagePath = await _imagesCacheService.GetCachedArtistImage(artist.Name);
            if (imagePath == null)
            {
                HubBackgroundImage = HUB_BACKGROUND_DEFAULT;
                imagePath = await _imagesCacheService.CacheAndGetArtistImage(artist.Name);
            }

            if (imagePath != null)
                HubBackgroundImage = imagePath;
            else
                _backgroundLoaded = false;
        }

        private async void TryLoadFirstTrackInfo(VKAudioWithImage track)
        {
            string imagePath = await _imagesCacheService.GetCachedAlbumImage(track.Title);
            if (imagePath == null)
                imagePath = await _imagesCacheService.CacheAndGetAlbumImage(track.Title, track.Artist);

            if (imagePath != null)
                track.ImageURL = imagePath;
        }

#endif

        private async void OnPlayRecommendedTracksCommand(Audio track)
        {
            if (track.Title == VKSAVER_SEE_ALSO_TEXT)
            {
                OnGoToRecommendedViewCommand();
                return;
            }

            await _playerService.PlayNewTracks(RecommendedTracksVK
                .Where(a => a.Title != VKSAVER_SEE_ALSO_TEXT)
                .Select(a => a.ToPlayerTrack()),
                RecommendedTracksVK.IndexOf(track));

#if !WINDOWS_UWP
            _navigationService.Navigate("PlayerView", null);
#endif
        }

        private async void OnPlayUserTracksCommand(Audio track)
        {
            if (track.Title == VKSAVER_SEE_ALSO_TEXT)
            {
                OnGoToUserContentCommand("audios");
                return;
            }

            List<IPlayerTrack> tracksToPlay = null;
            await Task.Run(() =>
            {
#if WINDOWS_UWP
                tracksToPlay = new List<IPlayerTrack>(UserTracks
                    .Where(a => a.Title != VKSAVER_SEE_ALSO_TEXT)
                    .Select(a => a.ToPlayerTrack()));
#else
                tracksToPlay = new List<IPlayerTrack>(UserTracks.Count + 1);
                tracksToPlay.Add(FirstTrack.VKTrack.ToPlayerTrack());
                tracksToPlay.AddRange(UserTracks.Select(a => a.ToPlayerTrack()));
#endif
            });

            if (tracksToPlay == null)
                return;

            IPlayerTrack plTrack = track.ToPlayerTrack();
            await _playerService.PlayNewTracks(tracksToPlay, tracksToPlay.IndexOf(plTrack));

#if !WINDOWS_UWP
            _navigationService.Navigate("PlayerView", null);
#endif
        }

        private bool _backgroundLoaded;
    }
}
