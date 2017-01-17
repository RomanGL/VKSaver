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
using VKSaver.Core.Services.Json;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class MainViewModel : ViewModelBase
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
        {
            _inTouch = inTouch;
            _lfClient = lfClient;
            _inTouchWrapper = inTouchWrapper;
            _navigationService = navigationService;
            _purchaseService = purchaseService;
            _playerService = playerService;
            _downloadsServiceHelper = downloadsServiceHelper;
            _imagesCacheService = imagesCacheService;
            _adsService = adsService;

            GoToTrackInfoCommand = new DelegateCommand<LastTrack>(OnGoToTrackInfoCommand);
            GoToArtistInfoCommand = new DelegateCommand<LastArtist>(OnGoToArtistInfoCommand);
            GoToTopTracksCommand = new DelegateCommand(OnGoToTopTracksCommand);
            GoToTopArtistsCommand = new DelegateCommand(OnGoTopArtistsCommand);
            GoToUserContentCommand = new DelegateCommand<string>(OnGoToUserContentCommand);
            GoToUserCommCommand = new DelegateCommand<string>(OnGoToUserCommCommand);
            GoToTransferViewCommand = new DelegateCommand<string>(OnGoToTransferViewCommand);
            GoToAboutViewCommand = new DelegateCommand(OnGoToAboutViewCommand);
            GoToRecommendedViewCommand = new DelegateCommand(OnGoToRecommendedViewCommand);
            GoToPlayerViewCommand = new DelegateCommand(OnGoToPlayerViewCommand);
            PlayRecommendedTracksCommand = new DelegateCommand<Audio>(OnPlayRecommendedTracksCommand);
            PlayUserTracksCommand = new DelegateCommand<Audio>(OnPlayUserTracksCommand);
            DownloadTrackCommand = new DelegateCommand<Audio>(OnDownloadTrackCommand);
            GoToSearchCommand = new DelegateCommand(OnGoToSearchCommand);
            GoToSettingsViewCommand = new DelegateCommand(OnGoToSettingsViewCommand);
            GoToNewsViewCommand = new DelegateCommand(OnGoToNewsViewCommand);
            GoToLibraryViewCommand = new DelegateCommand<string>(OnGoToLibraryViewCommand);
            GoToUploadFileViewCommand = new DelegateCommand(OnGoToUploadFileViewCommand);
            GoToPopularVKViewCommand = new DelegateCommand(OnGoToPopularVKViewCommand);

            NotImplementedCommand = new DelegateCommand(() => _navigationService.Navigate("AccessDeniedView", null));
        }

        public SimpleStateSupportCollection<Audio> UserTracks { get; private set; }

        public SimpleStateSupportCollection<LastArtist> TopArtistsLF { get; private set; }

        public SimpleStateSupportCollection<Audio> RecommendedTracksVK { get; private set; }

        [DoNotNotify]
        public DelegateCommand<LastTrack> GoToTrackInfoCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<LastArtist> GoToArtistInfoCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand GoToBlankPageCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand GoToTopTracksCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand GoToTopArtistsCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<string> GoToUserContentCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<string> GoToUserCommCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<string> GoToTransferViewCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand GoToAboutViewCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand GoToRecommendedViewCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand GoToPlayerViewCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<Audio> PlayRecommendedTracksCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<Audio> PlayUserTracksCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand GoToSettingsViewCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand NotImplementedCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<Audio> DownloadTrackCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand GoToSearchCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand GoToNewsViewCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<string> GoToLibraryViewCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand GoToUploadFileViewCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand GoToPopularVKViewCommand { get; private set; }

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

            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.Get(count: 10));
            if (response.IsError)
                throw new Exception(response.Error.ToString());
            else
            {
                if (response.Data.Items.Count == 0)
                    return new List<Audio>();

#if WINDOWS_UWP
                return response.Data.Items;
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

            var response = await _lfClient.Chart.GetTopArtistsAsync(itemsPerPage: 8);
            if (response.Success)
            {
#if !WINDOWS_UWP
                TryLoadTopArtistBackground(response.First());
#endif
                return response;
            }
            else
                throw new Exception("LFChartArtistsResponse isn't valid.");
        }

        private async Task<IEnumerable<Audio>> LoadRecommendedTracks()
        {
            if (RecommendedTracksVK.Any())
                return new List<Audio>();

            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.GetRecommendations(count: 10));
            if (response.IsError)
                throw new Exception(response.Error.ToString());
            else
                return response.Data.Items;
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

        private void OnGoToTrackInfoCommand(LastTrack audio) => _navigationService.Navigate("TrackInfoView",
            JsonConvert.SerializeObject(audio, _lastImageSetConverter));
        private void OnGoToArtistInfoCommand(LastArtist artist) => _navigationService.Navigate("ArtistInfoView",
            JsonConvert.SerializeObject(artist, _lastImageSetConverter));

        private void OnGoToUserContentCommand(string view) => _navigationService.Navigate("UserContentView", JsonConvert.SerializeObject(
                new KeyValuePair<string, string>(view, "0")));
        private void OnGoToUserCommCommand(string view) => _navigationService.Navigate("UserCommView", JsonConvert.SerializeObject(
                new KeyValuePair<string, string>(view, "0")));

        private void OnGoToLibraryViewCommand(string view) => _navigationService.Navigate("LibraryView", view);
        private void OnGoToUploadFileViewCommand() => _navigationService.Navigate("UploadFileView", null);
        private void OnGoToTransferViewCommand(string view) => _navigationService.Navigate("TransferView", view);
        private void OnGoToAboutViewCommand() => _navigationService.Navigate("AboutView", null);
        private void OnGoToSearchCommand() => _navigationService.Navigate("SelectSearchTypeView", null);
        private void OnGoToPlayerViewCommand() => _navigationService.Navigate("PlayerView", null);
        private void OnGoToSettingsViewCommand() => _navigationService.Navigate("SettingsView", null);

        private void OnGoToNewsViewCommand() => NavigateToPaidView("NewsMediaView");
        private void OnGoToTopTracksCommand() => NavigateToPaidView("TopTracksView");
        private void OnGoTopArtistsCommand() => NavigateToPaidView("TopArtistsView");
        private void OnGoToRecommendedViewCommand() => NavigateToPaidView("RecommendedView", "0");
        private void OnGoToPopularVKViewCommand() => NavigateToPaidView("PopularVKAudioView");

        private void NavigateToPaidView(string viewName, string parameter = null)
        {
            if (_purchaseService.IsFullVersionPurchased)
                _navigationService.Navigate(viewName, parameter);
            else
                _navigationService.Navigate("PurchaseView", JsonConvert.SerializeObject(
                    new KeyValuePair<string, string>(viewName, parameter)));
        }

        private async void OnPlayRecommendedTracksCommand(Audio track)
        {
            await _playerService.PlayNewTracks(RecommendedTracksVK.Select(a => a.ToPlayerTrack()),
                RecommendedTracksVK.IndexOf(track));
            _navigationService.Navigate("PlayerView", null);
        }

        private async void OnPlayUserTracksCommand(Audio track)
        {
            List<IPlayerTrack> tracksToPlay = null;
            await Task.Run(() =>
            {
#if WINDOWS_UWP
                tracksToPlay = new List<IPlayerTrack>(UserTracks.Select(a => a.ToPlayerTrack()));
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

            _navigationService.Navigate("PlayerView", null);
        }

        private async void OnDownloadTrackCommand(Audio track)
        {
            await _downloadsServiceHelper.StartDownloadingAsync(track.ToDownloadable());
        }

        private bool _backgroundLoaded;

        private readonly InTouch _inTouch;
        private readonly LastfmClient _lfClient;
        private readonly IInTouchWrapper _inTouchWrapper;
        private readonly INavigationService _navigationService;
        private readonly IPurchaseService _purchaseService;
        private readonly IPlayerService _playerService;
        private readonly IDownloadsServiceHelper _downloadsServiceHelper;
        private readonly IImagesCacheService _imagesCacheService;
        private readonly IAdsService _adsService;

        private static readonly LastImageSetConverter _lastImageSetConverter = new LastImageSetConverter();

        private const string HUB_BACKGROUND_DEFAULT = "ms-appx:///Assets/HubBackground.jpg";        
    }
}
