#if WINDOWS_UWP
using Prism.Commands;
using Prism.Windows.Navigation;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#elif ANDROID
using VKSaver.Core.Toolkit.Commands;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PropertyChanged;
using Newtonsoft.Json;
using VKSaver.Core.ViewModels.Collections;
using VKSaver.Core.ViewModels.Search;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Services;
using IF.Lastfm.Core.Objects;
using IF.Lastfm.Core.Api;
using VKSaver.Core.Services.Json;
using IF.Lastfm.Core.Api.Helpers;
using VKSaver.Core.Toolkit;
using NavigatedToEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatedToEventArgs;
using NavigatingFromEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatingFromEventArgs;
using VKSaver.Core.Toolkit.Navigation;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class ArtistInfoViewModel : VKSaverViewModel
    {
        public ArtistInfoViewModel(
            LastfmClient lfClient,
            INavigationService navigationService,
            ISettingsService settingsService, 
            IPurchaseService purchaseService,
            IImagesCacheService imagesCacheService)
        {
            _lfClient = lfClient;
            _navigationService = navigationService;
            _settingsService = settingsService;
            _purchaseService = purchaseService;
            _imagesCacheService = imagesCacheService;

            GoToTrackInfoCommand = new DelegateCommand<LastTrack>(OnGoToTrackInfoCommand);
            GoToSimilarArtistInfoCommand = new DelegateCommand<LastArtist>(OnGoToSimilarArtistInfoCommand);
            GoToAlbumInfoCommand = new DelegateCommand<LastAlbum>(OnGoToAlbumInfoCommand);
            FindArtistInVKCommand = new DelegateCommand(OnFindArtistInVKCommand);            
        }
        
        public SimpleStateSupportCollection<LastTrack> Tracks { get; private set; }

        public PaginatedCollection<LastAlbum> Albums { get; private set; }

        public SimpleStateSupportCollection<LastArtist> Similar { get; private set; }

        public LastArtist Artist { get; private set; }

        public string ArtistImage { get; private set; }

        public int LastPivotIndex { get; set; }

        [DoNotNotify]
        public DelegateCommand<LastTrack> GoToTrackInfoCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<LastArtist> GoToSimilarArtistInfoCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<LastAlbum> GoToAlbumInfoCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand FindArtistInVKCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (e.NavigationMode == NavigationMode.New)
                LastPivotIndex = 0;

            if (viewModelState.Count > 0)
            {
                Artist = JsonConvert.DeserializeObject<LastArtist>(
                    viewModelState[nameof(Artist)].ToString(), _lastImageSetConverter);
                Tracks = JsonConvert.DeserializeObject<SimpleStateSupportCollection<LastTrack>>(
                    viewModelState[nameof(Tracks)].ToString(), _lastImageSetConverter);
                Albums = JsonConvert.DeserializeObject<PaginatedCollection<LastAlbum>>(
                    viewModelState[nameof(Albums)].ToString(), _lastImageSetConverter);
                Similar = JsonConvert.DeserializeObject<SimpleStateSupportCollection<LastArtist>>(
                    viewModelState[nameof(Similar)].ToString(), _lastImageSetConverter);

                if (e.NavigationMode == NavigationMode.New)
                    LastPivotIndex = 0;
                else
                    LastPivotIndex = (int)viewModelState[nameof(LastPivotIndex)];

                Tracks.LoadItems = LoadTracks;
                Albums.LoadMoreItems = LoadMoreAlbums;
                Similar.LoadItems = LoadSimilar;

            }
            else
            {
                Artist = JsonConvert.DeserializeObject<LastArtist>(e.Parameter.ToString(), _lastImageSetConverter);
                Tracks = new SimpleStateSupportCollection<LastTrack>(LoadTracks);
                Albums = new PaginatedCollection<LastAlbum>(LoadMoreAlbums);
                Similar = new SimpleStateSupportCollection<LastArtist>(LoadSimilar);

                LastPivotIndex = 0;
            }

            Tracks.Load();
            Similar.Load();
            LoadArtistImage(Artist.Name);
            
            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                viewModelState[nameof(Tracks)] = JsonConvert.SerializeObject(Tracks.ToList(), _lastImageSetConverter);
                viewModelState[nameof(Albums)] = JsonConvert.SerializeObject(Albums.ToList(), _lastImageSetConverter);
                viewModelState[nameof(Similar)] = JsonConvert.SerializeObject(Similar.ToList(), _lastImageSetConverter);
                viewModelState[nameof(Artist)] = JsonConvert.SerializeObject(Artist, _lastImageSetConverter);
                viewModelState[nameof(LastPivotIndex)] = LastPivotIndex;
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async Task<IEnumerable<LastTrack>> LoadTracks()
        {
            if (Tracks.Any())
                return new List<LastTrack>();

            var response = await _lfClient.Artist.GetTopTracksAsync(Artist.Name, true);
            if (response.Success)
                return response;
            else
                throw new Exception();
        }

        private async Task<IEnumerable<LastAlbum>> LoadMoreAlbums(uint page)
        {
            var response = await _lfClient.Artist.GetTopAlbumsAsync(Artist.Name, true, (int)(page + 1));
            if (response.Success)
                return response;
            else
                throw new Exception();
        }

        private async Task<IEnumerable<LastArtist>> LoadSimilar()
        {
            if (Similar.Any())
                return new List<LastArtist>();

            PageResponse<LastArtist> response = null;
            if (String.IsNullOrEmpty(Artist.Mbid))
                response = await _lfClient.Artist.GetSimilarAsync(Artist.Name, true, 10);
            else
                response = await _lfClient.Artist.GetSimilarByMbidAsync(Artist.Mbid, true, 10);

            if (response.Success)
                return response;
            else
                throw new Exception();
        }

        private void OnGoToTrackInfoCommand(LastTrack audio)
        {
            _navigationService.Navigate("TrackInfoView", JsonConvert.SerializeObject(audio, _lastImageSetConverter));
        }

        private void OnGoToSimilarArtistInfoCommand(LastArtist artist)
        {
            if (_purchaseService.IsFullVersionPurchased)
                _navigationService.Navigate("ArtistInfoView", JsonConvert.SerializeObject(artist, _lastImageSetConverter));
            else
                _navigationService.Navigate("PurchaseView", JsonConvert.SerializeObject(
                    new KeyValuePair<string, string>("ArtistInfoView", JsonConvert.SerializeObject(artist, _lastImageSetConverter))));
        }

        private void OnGoToAlbumInfoCommand(LastAlbum album)
        {
            if (_purchaseService.IsFullVersionPurchased)
                _navigationService.Navigate("ArtistAlbumView", JsonConvert.SerializeObject(album, _lastImageSetConverter));
            else
                _navigationService.Navigate("PurchaseView", JsonConvert.SerializeObject(
                    new KeyValuePair<string, string>("ArtistAlbumView", JsonConvert.SerializeObject(album, _lastImageSetConverter))));
        }

        private void OnFindArtistInVKCommand()
        {
            if (Artist == null)
                return;

            _settingsService.Set(AudioSearchViewModel.PERFORMER_ONLY_PARAMETER_NAME, true);
            _navigationService.Navigate("AudioSearchView", JsonConvert.SerializeObject(
                new SearchNavigationParameter
                {
                    Query = Artist.Name
                }));
        }

        private async void LoadArtistImage(string artist)
        {
            string img = await _imagesCacheService.GetCachedArtistImage(artist);
            if (img == null)
            {
                ArtistImage = AppConstants.DEFAULT_PLAYER_BACKGROUND_IMAGE;
                img = await _imagesCacheService.CacheAndGetArtistImage(artist);

                if (img != null && artist == Artist?.Name)
                    ArtistImage = img;
            }
            else if (artist == Artist?.Name)
            {
                ArtistImage = img;
            }
        }
                
        private readonly LastfmClient _lfClient;
        private readonly INavigationService _navigationService;
        private readonly ISettingsService _settingsService;
        private readonly IPurchaseService _purchaseService;
        private readonly IImagesCacheService _imagesCacheService;

        private static readonly LastImageSetConverter _lastImageSetConverter = new LastImageSetConverter();
    }
}
