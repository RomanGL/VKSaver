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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Services.Json;
using VKSaver.Core.ViewModels.Collections;
using Windows.UI.Xaml.Navigation;
using IF.Lastfm.Core.Objects;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class TrackInfoViewModel : VKAudioImplementedViewModel
    {
        public TrackInfoViewModel(
            INavigationService navigationService, 
            InTouch inTouch,
            IInTouchWrapper inTouchWrapper, 
            IPlayerService playerService,
            IDownloadsServiceHelper downloadsServiceHelper, 
            IAppLoaderService appLoaderService,
            IDialogsService dialogsService, 
            ILocService locService,
            IImagesCacheService imagesCacheService,
            IPurchaseService purchaseService)
            : base(inTouch, appLoaderService, dialogsService, inTouchWrapper, downloadsServiceHelper,
                  playerService, locService, navigationService, purchaseService)
        {
            _imagesCacheService = imagesCacheService;

            ShowOtherTracksCommand = new DelegateCommand(OnShowOtherTracksCommand);
        }

        public string ArtistImage { get; private set; }

        public LastTrack Track { get; private set; }

        [DoNotNotify]
        public SimpleStateSupportCollection<Audio> VKTracks { get; private set; }

        [DoNotNotify]
        public DelegateCommand ShowOtherTracksCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            Track = JsonConvert.DeserializeObject<LastTrack>(e.Parameter.ToString(), _lastImageSetConverter);

            if (viewModelState.Count == 0)
            {
                VKTracks = new SimpleStateSupportCollection<Audio>(LoadVKTracks);
                VKTracks.Load();
            }
            else
            {
                VKTracks = JsonConvert.DeserializeObject<SimpleStateSupportCollection<Audio>>(
                    viewModelState[nameof(VKTracks)].ToString());
            }

            LoadArtistImage(Track.ArtistName);
            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (!suspending && e.NavigationMode == NavigationMode.New)
            {
                if (VKTracks != null && VKTracks.Count > 0)
                    viewModelState[nameof(VKTracks)] = JsonConvert.SerializeObject(VKTracks);
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        protected override IList GetSelectionList() => VKTracks;

        protected override IList<Audio> GetAudiosList() => VKTracks;

        protected override void OnReloadContentCommand()
        {
            AppBarItems.Clear();
            SecondaryItems.Clear();
            VKTracks.Refresh();
        }

        private async Task<IEnumerable<Audio>> LoadVKTracks()
        {
            if (Track == null || Track.ArtistName == null)
                return new List<Audio>(0);

            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.Search(new AudioSearchParams
            {
                AutoComplete = true,
                Count = 10,
                Query = $"{Track.ArtistName} - {Track.Name}"
            }));

            if (response.IsError)
                throw new Exception(response.Error.ToString());

            if (!VKTracks.Any() && response.Data.Items.Any())
                SetDefaultMode();

            return response.Data.Items;
        }

        private void OnShowOtherTracksCommand()
        {
            _navigationService.Navigate("AccessDeniedView", null);
        }

        private async void LoadArtistImage(string artist)
        {
            string img = await _imagesCacheService.GetCachedArtistImage(artist);
            if (img == null)
            {
                ArtistImage = AppConstants.DEFAULT_PLAYER_BACKGROUND_IMAGE;
                img = await _imagesCacheService.CacheAndGetArtistImage(artist);

                if (img != null && artist == Track?.ArtistName)
                    ArtistImage = img;
            }
            else if (artist == Track?.ArtistName)
            {
                ArtistImage = img;
            }
        }
        
        private readonly IImagesCacheService _imagesCacheService;

        private static readonly LastImageSetConverter _lastImageSetConverter = new LastImageSetConverter();
    }
}
