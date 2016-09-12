using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using ModernDev.InTouch;
using Newtonsoft.Json;
using OneTeam.SDK.Core;
using OneTeam.SDK.LastFm.Models.Audio;
using OneTeam.SDK.LastFm.Models.Response;
using OneTeam.SDK.LastFm.Services.Interfaces;
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

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class MainViewModel : ViewModelBase
    {
        public MainViewModel(InTouch inTouch, IInTouchWrapper inTouchWrapper, ILFService lfService,
            ISettingsService settingsService, INavigationService navigationService,
            IPurchaseService purchaseService, IPlayerService playerService,
            IDownloadsServiceHelper downloadsServiceHelper, IImagesCacheService imagesCacheService)
        {
            _inTouch = inTouch;
            _inTouchWrapper = inTouchWrapper;
            _lfService = lfService;
            _settingsService = settingsService;
            _navigationService = navigationService;
            _purchaseService = purchaseService;
            _playerService = playerService;
            _downloadsServiceHelper = downloadsServiceHelper;
            _imagesCacheService = imagesCacheService;

            GoToTrackInfoCommand = new DelegateCommand<LFAudioBase>(OnGoToTrackInfoCommand);
            GoToArtistInfoCommand = new DelegateCommand<LFArtistExtended>(OnGoToArtistInfoCommand);
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

            NotImplementedCommand = new DelegateCommand(() => _navigationService.Navigate("AccessDeniedView", null));
        }

        public SimpleStateSupportCollection<Audio> UserTracks { get; private set; }

        public SimpleStateSupportCollection<LFArtistExtended> TopArtistsLF { get; private set; }

        public SimpleStateSupportCollection<Audio> RecommendedTracksVK { get; private set; }

        [DoNotNotify]
        public DelegateCommand<LFAudioBase> GoToTrackInfoCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<LFArtistExtended> GoToArtistInfoCommand { get; private set; }

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

        public VKAudioWithImage FirstTrack { get; private set; }

        public string HubBackgroundImage { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            _backgroundLoaded = false;
            if (viewModelState.Count > 0)
            {
                UserTracks = JsonConvert.DeserializeObject<SimpleStateSupportCollection<Audio>>(
                    viewModelState[nameof(UserTracks)].ToString());
                FirstTrack = JsonConvert.DeserializeObject<VKAudioWithImage>(
                    viewModelState[nameof(FirstTrack)].ToString());
                TopArtistsLF = JsonConvert.DeserializeObject<SimpleStateSupportCollection<LFArtistExtended>>(
                    viewModelState[nameof(TopArtistsLF)].ToString());
                RecommendedTracksVK = JsonConvert.DeserializeObject<SimpleStateSupportCollection<Audio>>(
                    viewModelState[nameof(RecommendedTracksVK)].ToString());

                UserTracks.LoadItems = LoadUserTracks;
                TopArtistsLF.LoadItems = LoadTopArtists;
                RecommendedTracksVK.LoadItems = LoadRecommendedTracks;
            }
            else
            {
                UserTracks = new SimpleStateSupportCollection<Audio>(LoadUserTracks);
                TopArtistsLF = new SimpleStateSupportCollection<LFArtistExtended>(LoadTopArtists);
                RecommendedTracksVK = new SimpleStateSupportCollection<Audio>(LoadRecommendedTracks);
            }

            UserTracks.Load();
            TopArtistsLF.Load();
            RecommendedTracksVK.Load();

            TryLoadBackground(FirstTrack);
            //TryLoadTopArtistBackground(TopArtistsLF?.FirstOrDefault());

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                viewModelState[nameof(FirstTrack)] = JsonConvert.SerializeObject(FirstTrack);
                viewModelState[nameof(UserTracks)] = JsonConvert.SerializeObject(UserTracks.ToList());
                viewModelState[nameof(TopArtistsLF)] = JsonConvert.SerializeObject(TopArtistsLF.ToList());
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

                FirstTrack = new VKAudioWithImage
                {
                    VKTrack = response.Data.Items[0]
                };

                TryLoadFirstTrackInfo(FirstTrack);
                TryLoadBackground(FirstTrack);
                return response.Data.Items.Skip(1);
            }
        }

        private async Task<IEnumerable<LFArtistExtended>> LoadTopArtists()
        {
            if (TopArtistsLF.Any())
                return new List<LFArtistExtended>();

            var request = new Request<LFChartArtistsResponse>("chart.getTopArtists",
                new Dictionary<string, string> { { "limit", "8" } });
            var response = await _lfService.ExecuteRequestAsync(request);

            if (response.IsValid())
            {
                TryLoadTopArtistBackground(response.Data.Artists[0]);
                return response.Data.Artists;
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

        private async void TryLoadTopArtistBackground(LFArtistExtended artist)
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

        private void OnGoToTrackInfoCommand(LFAudioBase audio)
        {
            _navigationService.Navigate("TrackInfoView", JsonConvert.SerializeObject(audio));
        }

        private void OnGoToArtistInfoCommand(LFArtistExtended artist)
        {
            _navigationService.Navigate("ArtistInfoView", JsonConvert.SerializeObject(artist));
        }

        private void OnGoToNewsViewCommand()
        {
            if (_purchaseService.IsFullVersionPurchased)
                _navigationService.Navigate("NewsMediaView", null);
            else
                _navigationService.Navigate("PurchaseView", JsonConvert.SerializeObject(
                    new KeyValuePair<string, string>("NewsMediaView", null)));
        }

        private void OnGoToTopTracksCommand()
        {
            if (_purchaseService.IsFullVersionPurchased)
                _navigationService.Navigate("TopTracksView", null);
            else
                _navigationService.Navigate("PurchaseView", JsonConvert.SerializeObject(
                    new KeyValuePair<string, string>("TopTracksView", null)));
        }

        private void OnGoTopArtistsCommand()
        {
            if (_purchaseService.IsFullVersionPurchased)
                _navigationService.Navigate("TopArtistsView", null);
            else
                _navigationService.Navigate("PurchaseView", JsonConvert.SerializeObject(
                    new KeyValuePair<string, string>("TopArtistsView", null)));
        }

        private void OnGoToUserContentCommand(string view)
        {
            _navigationService.Navigate("UserContentView", JsonConvert.SerializeObject(
                new KeyValuePair<string, string>(view, "0")));
        }

        private void OnGoToUserCommCommand(string view)
        {
            _navigationService.Navigate("UserCommView", JsonConvert.SerializeObject(
                new KeyValuePair<string, string>(view, "0")));
        }

        private void OnGoToTransferViewCommand(string view)
        {
            _navigationService.Navigate("TransferView", view);
        }

        private void OnGoToAboutViewCommand()
        {
            _navigationService.Navigate("AboutView", null);
        }

        private void OnGoToSearchCommand()
        {
            _navigationService.Navigate("SelectSearchTypeView", null);
        }

        private void OnGoToRecommendedViewCommand()
        {
            if (_purchaseService.IsFullVersionPurchased)
                _navigationService.Navigate("RecommendedView", "0");
            else
                _navigationService.Navigate("PurchaseView", JsonConvert.SerializeObject(
                    new KeyValuePair<string, string>("RecommendedView", "0")));
        }

        private void OnGoToPlayerViewCommand()
        {
            _navigationService.Navigate("PlayerView", null);
        }

        private void OnGoToSettingsViewCommand()
        {
            _navigationService.Navigate("SettingsView", null);
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
                tracksToPlay = new List<IPlayerTrack>(UserTracks.Count + 1);
                tracksToPlay.Add(FirstTrack.VKTrack.ToPlayerTrack());
                tracksToPlay.AddRange(UserTracks.Select(a => a.ToPlayerTrack()));
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
        private readonly IInTouchWrapper _inTouchWrapper;
        private readonly ILFService _lfService;
        private readonly ISettingsService _settingsService;
        private readonly INavigationService _navigationService;
        private readonly IPurchaseService _purchaseService;
        private readonly IPlayerService _playerService;
        private readonly IDownloadsServiceHelper _downloadsServiceHelper;
        private readonly IImagesCacheService _imagesCacheService;

        private const string HUB_BACKGROUND_DEFAULT = "ms-appx:///Assets/HubBackground.jpg";        
    }
}
