using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Newtonsoft.Json;
using OneTeam.SDK.Core;
using OneTeam.SDK.LastFm.Models.Audio;
using OneTeam.SDK.LastFm.Models.Common;
using OneTeam.SDK.LastFm.Models.Response;
using OneTeam.SDK.LastFm.Services.Interfaces;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Extensions;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Search;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class ArtistAlbumViewModel : ViewModelBase
    {
        public ArtistAlbumViewModel(
            ILFService lfService, 
            INavigationService navigationService,
            ISettingsService settingsService,
            IImagesCacheService imagesCacheService,
            ILocService locService)
        {
            _lfService = lfService;
            _navigationService = navigationService;
            _settingsService = settingsService;
            _imagesCacheService = imagesCacheService;
            _locService = locService;

            GoToTrackInfoCommand = new DelegateCommand<LFAudioBase>(OnGoToTrackInfoCommand);
            FindArtistInVKCommand = new DelegateCommand(OnFindArtistInVKCommand);
            ReloadAlbumCommand = new DelegateCommand(OnReloadAlbumCommand);
        }

        public string ArtistImage { get; private set; }

        public string AlbumImage { get; private set; }

        public LFAlbumBase AlbumBase { get; private set; }

        public LFAlbumExtended Album { get; private set; }

        public int LastPivotIndex { get; set; }

        public ContentState TracksState { get; private set; }

        public ContentState WikiState { get; private set; }

        [DoNotNotify]
        public DelegateCommand<LFAudioBase> GoToTrackInfoCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand FindArtistInVKCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand ReloadAlbumCommand { get; private set; }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (e.NavigationMode == NavigationMode.New)
                LastPivotIndex = 0;

            if (viewModelState.Count > 0)
            {
                AlbumBase = JsonConvert.DeserializeObject<LFAlbumBase>(
                    viewModelState[nameof(AlbumBase)].ToString());
                AlbumImage = AlbumBase.LargeImage.URL;

                if (e.NavigationMode != NavigationMode.New)
                    LastPivotIndex = (int)viewModelState[nameof(LastPivotIndex)];

                object albumJson = null;
                if (viewModelState.TryGetValue(nameof(Album), out albumJson))
                {
                    Album = JsonConvert.DeserializeObject<LFAlbumExtended>(albumJson.ToString());
                    TracksState = Album.Tracks != null &&
                        Album.Tracks.Tracks != null &&
                        Album.Tracks.Tracks.Count > 0
                        ? ContentState.Normal : ContentState.NoData;
                    WikiState = Album.Wiki != null ? ContentState.Normal : ContentState.NoData;
                }
            }
            else
            {
                AlbumBase = JsonConvert.DeserializeObject<LFAlbumBase>(e.Parameter.ToString());
                AlbumImage = AlbumBase.LargeImage.URL;
            }

            if (Album == null)
                await LoadAlbumInfo();

            LoadArtistImage(AlbumBase.Artist.Name);
            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                viewModelState[nameof(ArtistImage)] = ArtistImage;
                viewModelState[nameof(AlbumBase)] = JsonConvert.SerializeObject(AlbumBase);
                viewModelState[nameof(LastPivotIndex)] = LastPivotIndex;

                if (Album != null)
                    viewModelState[nameof(Album)] = JsonConvert.SerializeObject(Album);
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async Task LoadAlbumInfo()
        {
            TracksState = ContentState.Loading;
            WikiState = ContentState.Loading;
            
            var parameters = new Dictionary<string, string>
            {
                { "lang", _locService.CurrentLanguage.ToLastFmLang() }
            };
            if (String.IsNullOrEmpty(AlbumBase.MBID))
            {
                parameters["album"] = AlbumBase.Name;
                parameters["artist"] = AlbumBase.Artist.Name;
            }
            else
                parameters["mbid"] = AlbumBase.MBID;

            var request = new Request<LFAlbumInfoResponse>("album.getInfo", parameters);
            var response = await _lfService.ExecuteRequestAsync(request);

            if (response.IsValid())
            {
                Album = response.Album;

                if (Album.Tracks != null && Album.Tracks.IsValid() && Album.Tracks.Tracks.Count > 0)
                {
                    foreach (var track in Album.Tracks.Tracks)
                    {
                        track.Images = new List<LFImage>(1)
                        {
                            new LFImage { URL = AlbumImage, Size = LFImageSize.Large }
                        };
                    }

                    TracksState = ContentState.Normal;
                }
                else
                    TracksState = ContentState.NoData;

                if (Album.Wiki != null)
                {
                    Album.Wiki.Content = Album.Wiki.Content.Split(
                        new string[] { "<a href" }, StringSplitOptions.RemoveEmptyEntries)[0];

                    WikiState = ContentState.Normal;
                }
                else
                    WikiState = ContentState.NoData;
            }
            else
            {
                TracksState = ContentState.Error;
                WikiState = ContentState.Error;
            }
        }

        private async void OnReloadAlbumCommand()
        {
            await LoadAlbumInfo();
        }

        private void OnGoToTrackInfoCommand(LFAudioBase audio)
        {
            _navigationService.Navigate("TrackInfoView", JsonConvert.SerializeObject(audio));
        }

        private void OnFindArtistInVKCommand()
        {
            _settingsService.Set(AudioSearchViewModel.PERFORMER_ONLY_PARAMETER_NAME, true);
            _navigationService.Navigate("AudioSearchView", JsonConvert.SerializeObject(
                new SearchNavigationParameter
                {
                    Query = AlbumBase.Artist.Name
                }));
        }

        private async void LoadArtistImage(string artist)
        {
            string img = await _imagesCacheService.GetCachedArtistImage(artist);
            if (img == null)
            {
                ArtistImage = AppConstants.DEFAULT_PLAYER_BACKGROUND_IMAGE;
                img = await _imagesCacheService.CacheAndGetArtistImage(artist);

                if (img != null && artist == AlbumBase?.Artist?.Name)
                    ArtistImage = img;
            }
            else if (artist == AlbumBase?.Artist?.Name)
            {
                ArtistImage = img;
            }
        }

        private readonly ILFService _lfService;
        private readonly INavigationService _navigationService;
        private readonly ISettingsService _settingsService;
        private readonly IImagesCacheService _imagesCacheService;
        private readonly ILocService _locService;
    }
}
