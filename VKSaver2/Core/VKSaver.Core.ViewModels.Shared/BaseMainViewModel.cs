
using System.Collections.Generic;
using IF.Lastfm.Core.Api;
#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#else
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#endif

using IF.Lastfm.Core.Objects;
using ModernDev.InTouch;
using Newtonsoft.Json;
using PropertyChanged;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Services.Json;
using VKSaver.Core.ViewModels.Common;


namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public abstract class BaseMainViewModel : ViewModelBase
    {
        protected BaseMainViewModel(
            InTouch inTouch,
            LastfmClient lfClient,
            IInTouchWrapper inTouchWrapper,
            INavigationService navigationService,
            IDownloadsServiceHelper downloadsServiceHelper,
            IPurchaseService purchaseService,
            IAdsService adsService,
            IPlayerService playerService,
            IImagesCacheService imagesCacheService)
        {
            _inTouch = inTouch;
            _lfClient = lfClient;
            _inTouchWrapper = inTouchWrapper;
            _navigationService = navigationService;
            _downloadsServiceHelper = downloadsServiceHelper;
            _purchaseService = purchaseService;
            _adsService = adsService;
            _playerService = playerService;
            _imagesCacheService = imagesCacheService;

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
            
            DownloadTrackCommand = new DelegateCommand<Audio>(OnDownloadTrackCommand);
            GoToSearchCommand = new DelegateCommand(OnGoToSearchCommand);
            GoToSettingsViewCommand = new DelegateCommand(OnGoToSettingsViewCommand);
            GoToNewsViewCommand = new DelegateCommand(OnGoToNewsViewCommand);
            GoToLibraryViewCommand = new DelegateCommand<string>(OnGoToLibraryViewCommand);
            GoToUploadFileViewCommand = new DelegateCommand(OnGoToUploadFileViewCommand);
            GoToPopularVKViewCommand = new DelegateCommand(OnGoToPopularVKViewCommand);

            NotImplementedCommand = new DelegateCommand(() => _navigationService.Navigate("AccessDeniedView", null));
        }

        [DoNotNotify]
        public DelegateCommand<LastTrack> GoToTrackInfoCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<LastArtist> GoToArtistInfoCommand { get; private set; }

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

        #region Protected methods

        protected void OnGoToRecommendedViewCommand() => NavigateToPaidView("RecommendedView", "0");
        protected void OnGoToUserContentCommand(string view) => _navigationService.Navigate("UserContentView", JsonConvert.SerializeObject(
            new KeyValuePair<string, string>(view, "0")));

        protected void NavigateToPaidView(string viewName, string parameter = null)
        {
            if (_purchaseService.IsFullVersionPurchased)
                _navigationService.Navigate(viewName, parameter);
            else
                _navigationService.Navigate("PurchaseView", JsonConvert.SerializeObject(
                    new KeyValuePair<string, string>(viewName, parameter)));
        }

        #endregion

        #region Private methods

        private void OnGoToTrackInfoCommand(LastTrack audio) => _navigationService.Navigate("TrackInfoView",
            JsonConvert.SerializeObject(audio, _lastImageSetConverter));

        private void OnGoToArtistInfoCommand(LastArtist artist)
        {
            if (artist.Name == VKSAVER_SEE_ALSO_TEXT)
                OnGoTopArtistsCommand();
            else
                _navigationService.Navigate("ArtistInfoView",
                    JsonConvert.SerializeObject(artist, _lastImageSetConverter));
        }

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
        private void OnGoToPopularVKViewCommand() => NavigateToPaidView("PopularVKAudioView");

        private async void OnDownloadTrackCommand(Audio track)
        {
            await _downloadsServiceHelper.StartDownloadingAsync(track.ToDownloadable());
        }

        #endregion

        protected readonly InTouch _inTouch;
        protected readonly LastfmClient _lfClient;
        protected readonly IInTouchWrapper _inTouchWrapper;
        protected readonly INavigationService _navigationService;
        protected readonly IDownloadsServiceHelper _downloadsServiceHelper;
        protected readonly IPurchaseService _purchaseService;
        protected readonly IAdsService _adsService;
        protected readonly IPlayerService _playerService;
        protected readonly IImagesCacheService _imagesCacheService;

        protected static readonly LastImageSetConverter _lastImageSetConverter = new LastImageSetConverter();
        protected const string HUB_BACKGROUND_DEFAULT = "ms-appx:///Assets/HubBackground.jpg";
        public const string VKSAVER_SEE_ALSO_TEXT = "VKSaver_SeeAlso_123";
    }
}
