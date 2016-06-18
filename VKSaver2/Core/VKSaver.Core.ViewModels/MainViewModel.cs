using Microsoft.Practices.Prism.StoreApps;
using OneTeam.SDK.Core.Services.Interfaces;
using OneTeam.SDK.LastFm.Models.Audio;
using OneTeam.SDK.LastFm.Services.Interfaces;
using OneTeam.SDK.VK.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;
using VKSaver.Core.ViewModels.Collections;
using OneTeam.SDK.Core;
using OneTeam.SDK.LastFm.Models.Response;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Newtonsoft.Json;
using VKSaver.Core.Services.Interfaces;
using Windows.UI.Xaml.Navigation;
using OneTeam.SDK.VK.Models.Audio;
using OneTeam.SDK.VK.Models.Common;
using VKSaver.Core.ViewModels.Common;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class MainViewModel : ViewModelBase
    {
        public MainViewModel(IVKService vkService, ILFService lfService, 
            ISettingsService settingsService, INavigationService navigationService,
            IPurchaseService purchaseService, IPlayerService playerService)
        {
            _vkService = vkService;
            _lfService = lfService;
            _settingsService = settingsService;
            _navigationService = navigationService;
            _purchaseService = purchaseService;
            _playerService = playerService;
                        
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
            PlayRecommendedTracksCommand = new DelegateCommand<VKAudio>(OnPlayRecommendedTracksCommand);

            NotImplementedCommand = new DelegateCommand(() => _navigationService.Navigate("AccessDeniedView", null));
        }
        
        public SimpleStateSupportCollection<LFAudioBase> TopTracksLF { get; private set; }
        
        public SimpleStateSupportCollection<LFArtistExtended> TopArtistsLF { get; private set; }
        
        public SimpleStateSupportCollection<VKAudio> RecommendedTracksVK { get; private set; }

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
        public DelegateCommand<VKAudio> PlayRecommendedTracksCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand NotImplementedCommand { get; private set; }

        public LFAudioBase TopTrack { get; private set; }

        public string TopTrackArtistImageURL { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (viewModelState.Count > 0)
            {
                TopTrack = JsonConvert.DeserializeObject<LFAudioBase>(
                    viewModelState[nameof(TopTrack)].ToString());
                TopTracksLF = JsonConvert.DeserializeObject<SimpleStateSupportCollection<LFAudioBase>>(
                    viewModelState[nameof(TopTracksLF)].ToString());
                TopArtistsLF = JsonConvert.DeserializeObject<SimpleStateSupportCollection<LFArtistExtended>>(
                    viewModelState[nameof(TopArtistsLF)].ToString());
                RecommendedTracksVK = JsonConvert.DeserializeObject<SimpleStateSupportCollection<VKAudio>>(
                    viewModelState[nameof(RecommendedTracksVK)].ToString());

                TopTracksLF.LoadItems = LoadTopTracks;
                TopArtistsLF.LoadItems = LoadTopArtists;
                RecommendedTracksVK.LoadItems = LoadRecommendedTracks;
            }
            else
            {
                TopTracksLF = new SimpleStateSupportCollection<LFAudioBase>(LoadTopTracks);
                TopArtistsLF = new SimpleStateSupportCollection<LFArtistExtended>(LoadTopArtists);
                RecommendedTracksVK = new SimpleStateSupportCollection<VKAudio>(LoadRecommendedTracks);
            }

            TopTracksLF.Load();
            TopArtistsLF.Load();
            RecommendedTracksVK.Load();

            TryLoadBackground();
            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                viewModelState[nameof(TopTrack)] = JsonConvert.SerializeObject(TopTrack);
                viewModelState[nameof(TopTracksLF)] = JsonConvert.SerializeObject(TopTracksLF.ToList());
                viewModelState[nameof(TopArtistsLF)] = JsonConvert.SerializeObject(TopArtistsLF.ToList());
                viewModelState[nameof(RecommendedTracksVK)] = JsonConvert.SerializeObject(RecommendedTracksVK.ToList());
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async Task<IEnumerable<LFAudioBase>> LoadTopTracks()
        {
            if (TopTracksLF.Any())
                return new List<LFAudioBase>();

            var request = new Request<LFChartTracksResponse>("chart.getTopTracks", 
                new Dictionary<string, string> { { "limit", "10" } });
            var response = await _lfService.ExecuteRequestAsync(request);

            if (response.IsValid())
            {
                TopTrack = response.Items.Tracks[0];
                TryLoadBackground();
                return response.Items.Tracks.Skip(1);
            }
            else
                throw new Exception("LFChartTracksResponse isn't valid.");
        } 

        private async Task<IEnumerable<LFArtistExtended>> LoadTopArtists()
        {
            if (TopArtistsLF.Any())
                return new List<LFArtistExtended>();

            var request = new Request<LFChartArtistsResponse>("chart.getTopArtists",
                new Dictionary<string, string> { { "limit", "8" } });
            var response = await _lfService.ExecuteRequestAsync(request);

            if (response.IsValid())
                return response.Data.Artists;
            else
                throw new Exception("LFChartArtistsResponse isn't valid.");
        }

        private async Task<IEnumerable<VKAudio>> LoadRecommendedTracks()
        {
            if (RecommendedTracksVK.Any())
                return new List<VKAudio>();

            var request = new Request<VKCountedItemsObject<VKAudio>>("audio.getRecommendations",
                new Dictionary<string, string> { { "count", "10" } });
            var response = await _vkService.ExecuteRequestAsync(request);

            if (response.IsSuccess)
                return response.Response.Items;
            else
                throw new Exception(response.Error.ToString());
        }

        private async void TryLoadBackground()
        {
            TopTrackArtistImageURL = HUB_BACKGROUND_DEFAULT;
        }

        private void OnGoToTrackInfoCommand(LFAudioBase audio)
        {
            _navigationService.Navigate("TrackInfoView", JsonConvert.SerializeObject(audio));
        }

        private void OnGoToArtistInfoCommand(LFArtistExtended artist)
        {
            _navigationService.Navigate("ArtistInfoView", JsonConvert.SerializeObject(artist));
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

        private async void OnPlayRecommendedTracksCommand(VKAudio track)
        {
            await _playerService.PlayNewTracks(RecommendedTracksVK.Select(a => a.ToPlayerTrack()),
                RecommendedTracksVK.IndexOf(track));
            _navigationService.Navigate("PlayerView", null);
        }

        private readonly IVKService _vkService;
        private readonly ILFService _lfService;
        private readonly ISettingsService _settingsService;
        private readonly INavigationService _navigationService;
        private readonly IPurchaseService _purchaseService;
        private readonly IPlayerService _playerService;

        private const string HUB_BACKGROUND_DEFAULT = "ms-appx:///Assets/HubBackground.jpg";
    }
}
