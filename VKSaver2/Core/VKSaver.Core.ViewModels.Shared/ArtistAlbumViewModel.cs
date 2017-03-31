#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#elif ANDROID
using VKSaver.Core.Toolkit.Commands;
#endif

using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Extensions;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Services.Json;
using VKSaver.Core.ViewModels.Search;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using VKSaver.Core.Toolkit;
using NavigatedToEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatedToEventArgs;
using NavigatingFromEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatingFromEventArgs;
using VKSaver.Core.Toolkit.Navigation;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class ArtistAlbumViewModel : VKSaverViewModel
    {
        public ArtistAlbumViewModel(
            LastfmClient lfClient,
            INavigationService navigationService,
            ISettingsService settingsService,
            IImagesCacheService imagesCacheService,
            ILocService locService)
        {
            _lfClient = lfClient;
            _navigationService = navigationService;
            _settingsService = settingsService;
            _imagesCacheService = imagesCacheService;
            _locService = locService;

            GoToTrackInfoCommand = new DelegateCommand<LastTrack>(OnGoToTrackInfoCommand);
            FindArtistInVKCommand = new DelegateCommand(OnFindArtistInVKCommand);
            ReloadAlbumCommand = new DelegateCommand(OnReloadAlbumCommand);
        }

        public string ArtistImage { get; private set; }

        public Uri AlbumImage { get; private set; }

        public LastAlbum AlbumBase { get; private set; }

        public LastAlbum Album { get; private set; }

        public int LastPivotIndex { get; set; }

        public ContentState TracksState { get; private set; }

        public ContentState WikiState { get; private set; }

        [DoNotNotify]
        public DelegateCommand<LastTrack> GoToTrackInfoCommand { get; private set; }

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
                AlbumBase = JsonConvert.DeserializeObject<LastAlbum>(
                    viewModelState[nameof(AlbumBase)].ToString(), _lastImageSetConverter);
                AlbumImage = AlbumBase.Images.Large;

                if (e.NavigationMode != NavigationMode.New)
                    LastPivotIndex = (int)viewModelState[nameof(LastPivotIndex)];

                object albumJson = null;
                if (viewModelState.TryGetValue(nameof(Album), out albumJson))
                {
                    Album = JsonConvert.DeserializeObject<LastAlbum>(albumJson.ToString(), _lastImageSetConverter);
                    TracksState = Album.Tracks != null &&
                        Album.Tracks != null &&
                        Album.Tracks.Any()
                        ? ContentState.Normal : ContentState.NoData;
                    WikiState = Album.Wiki != null ? ContentState.Normal : ContentState.NoData;
                }
            }
            else
            {
                AlbumBase = JsonConvert.DeserializeObject<LastAlbum>(e.Parameter.ToString(), _lastImageSetConverter);
                AlbumImage = AlbumBase.Images.Large;
            }

            if (Album == null)
                await LoadAlbumInfo();

            LoadArtistImage(AlbumBase.ArtistName);
            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                viewModelState[nameof(ArtistImage)] = ArtistImage;
                viewModelState[nameof(AlbumBase)] = JsonConvert.SerializeObject(AlbumBase, _lastImageSetConverter);
                viewModelState[nameof(LastPivotIndex)] = LastPivotIndex;

                if (Album != null)
                    viewModelState[nameof(Album)] = JsonConvert.SerializeObject(Album, _lastImageSetConverter);
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async Task LoadAlbumInfo()
        {
            TracksState = ContentState.Loading;
            WikiState = ContentState.Loading;

            LastResponse<LastAlbum> response = null;
            if (String.IsNullOrEmpty(AlbumBase.Mbid))
                response = await _lfClient.Album.GetInfoAsync(AlbumBase.ArtistName, AlbumBase.Name, true, _locService.CurrentLanguage.ToLastFmLang());
            else
                response = await _lfClient.Album.GetInfoByMbidAsync(AlbumBase.Mbid, true, _locService.CurrentLanguage.ToLastFmLang());

            if (response.Success)
            {
                if (response.Content.Tracks != null && response.Content.Tracks.Any())
                {
                    var tracksList = response.Content.Tracks.ToList();
                    foreach (var track in tracksList)
                    {
                        track.Images = response.Content.Images;
                    }
                    response.Content.Tracks = tracksList;

                    TracksState = ContentState.Normal;
                }
                else
                    TracksState = ContentState.NoData;

                Album = response.Content;

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

        private void OnGoToTrackInfoCommand(LastTrack audio)
        {
            _navigationService.Navigate("TrackInfoView", JsonConvert.SerializeObject(audio, _lastImageSetConverter));
        }

        private void OnFindArtistInVKCommand()
        {
            _settingsService.Set(AudioSearchViewModel.PERFORMER_ONLY_PARAMETER_NAME, true);
            _navigationService.Navigate("AudioSearchView", JsonConvert.SerializeObject(
                new SearchNavigationParameter
                {
                    Query = AlbumBase.ArtistName
                }));
        }

        private async void LoadArtistImage(string artist)
        {
            string img = await _imagesCacheService.GetCachedArtistImage(artist);
            if (img == null)
            {
                ArtistImage = AppConstants.DEFAULT_PLAYER_BACKGROUND_IMAGE;
                img = await _imagesCacheService.CacheAndGetArtistImage(artist);

                if (img != null && artist == AlbumBase?.ArtistName)
                    ArtistImage = img;
            }
            else if (artist == AlbumBase?.ArtistName)
            {
                ArtistImage = img;
            }
        }

        private readonly LastfmClient _lfClient;
        private readonly INavigationService _navigationService;
        private readonly ISettingsService _settingsService;
        private readonly IImagesCacheService _imagesCacheService;
        private readonly ILocService _locService;

        private static readonly LastImageSetConverter _lastImageSetConverter = new LastImageSetConverter();        
    }
}
