using Microsoft.Practices.Prism.StoreApps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;
using OneTeam.SDK.LastFm.Models.Audio;
using Newtonsoft.Json;
using VKSaver.Core.ViewModels.Collections;
using System.Collections;
using OneTeam.SDK.LastFm.Services.Interfaces;
using OneTeam.SDK.Core;
using OneTeam.SDK.LastFm.Models.Response;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Windows.UI.Xaml.Navigation;
using VKSaver.Core.ViewModels.Search;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class ArtistInfoViewModel : ViewModelBase
    {
        public ArtistInfoViewModel(ILFService lfService, INavigationService navigationService,
            ISettingsService settingsService, IPurchaseService purchaseService)
        {
            _lfService = lfService;
            _navigationService = navigationService;
            _settingsService = settingsService;
            _purchaseService = purchaseService;

            GoToTrackInfoCommand = new DelegateCommand<LFAudioBase>(OnGoToTrackInfoCommand);
            GoToSimilarArtistInfoCommand = new DelegateCommand<LFSimilarArtist>(OnGoToSimilarArtistInfoCommand);
            GoToAlbumInfoCommand = new DelegateCommand<LFAlbumBase>(OnGoToAlbumInfoCommand);
            FindArtistInVKCommand = new DelegateCommand(OnFindArtistInVKCommand);
        }
        
        public SimpleStateSupportCollection<LFAudioBase> Tracks { get; private set; }

        public PaginatedCollection<LFAlbumBase> Albums { get; private set; }

        public SimpleStateSupportCollection<LFSimilarArtist> Similar { get; private set; }

        public LFArtistExtended Artist { get; private set; }

        public int LastPivotIndex { get; set; }

        [DoNotNotify]
        public DelegateCommand<LFAudioBase> GoToTrackInfoCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<LFSimilarArtist> GoToSimilarArtistInfoCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<LFAlbumBase> GoToAlbumInfoCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand FindArtistInVKCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (e.NavigationMode == NavigationMode.New)
                LastPivotIndex = 0;

            if (viewModelState.Count > 0)
            {
                Artist = JsonConvert.DeserializeObject<LFArtistExtended>(
                    viewModelState[nameof(Artist)].ToString());
                Tracks = JsonConvert.DeserializeObject<SimpleStateSupportCollection<LFAudioBase>>(
                    viewModelState[nameof(Tracks)].ToString());
                Albums = JsonConvert.DeserializeObject<PaginatedCollection<LFAlbumBase>>(
                    viewModelState[nameof(Albums)].ToString());
                Similar = JsonConvert.DeserializeObject<SimpleStateSupportCollection<LFSimilarArtist>>(
                    viewModelState[nameof(Similar)].ToString());

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
                Artist = JsonConvert.DeserializeObject<LFArtistExtended>(e.Parameter.ToString());
                Tracks = new SimpleStateSupportCollection<LFAudioBase>(LoadTracks);
                Albums = new PaginatedCollection<LFAlbumBase>(LoadMoreAlbums);
                Similar = new SimpleStateSupportCollection<LFSimilarArtist>(LoadSimilar);

                LastPivotIndex = 0;
            }

            Tracks.Load();
            Similar.Load();

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                viewModelState[nameof(Tracks)] = JsonConvert.SerializeObject(Tracks.ToList());
                viewModelState[nameof(Albums)] = JsonConvert.SerializeObject(Albums.ToList());
                viewModelState[nameof(Similar)] = JsonConvert.SerializeObject(Similar.ToList());
                viewModelState[nameof(Artist)] = JsonConvert.SerializeObject(Artist);
                viewModelState[nameof(LastPivotIndex)] = LastPivotIndex;
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async Task<IEnumerable<LFAudioBase>> LoadTracks()
        {
            if (Tracks.Any())
                return new List<LFAudioBase>();

            var parameters = new Dictionary<string, string>
            {
                { "limit", "20" },
                { "autocorrect", "1" }
            };
            if (String.IsNullOrEmpty(Artist.MBID))
                parameters["artist"] = Artist.Name;
            else
                parameters["mbid"] = Artist.MBID;

            var request = new Request<LFTopTracksResponse>("artist.getTopTracks", parameters);
            var response = await _lfService.ExecuteRequestAsync(request);

            if (response.IsValid())
            {
                return response.Data.Tracks;
            }
            else
                throw new Exception("LFTopTracksResponse isn't valid.");
        }

        private async Task<IEnumerable<LFAlbumBase>> LoadMoreAlbums(uint page)
        {
            var parameters = new Dictionary<string, string>
            {
                { "limit", "20" },
                { "autocorrect", "1" },
                { "page", (page + 1).ToString() }
            };
            if (String.IsNullOrEmpty(Artist.MBID))
                parameters["artist"] = Artist.Name;
            else
                parameters["mbid"] = Artist.MBID;

            var request = new Request<LFTopAlbumsResponse>("artist.getTopAlbums", parameters);
            var response = await _lfService.ExecuteRequestAsync(request);

            if (response.IsValid())
            {
                return response.Data.Albums;
            }
            else
                throw new Exception("LFTopAlbumsResponse isn't valid.");
        }

        private async Task<IEnumerable<LFSimilarArtist>> LoadSimilar()
        {
            if (Similar.Any())
                return new List<LFSimilarArtist>();

            var parameters = new Dictionary<string, string>
            {
                { "limit", "10" },
                { "autocorrect", "1" }
            };
            if (String.IsNullOrEmpty(Artist.MBID))
                parameters["artist"] = Artist.Name;
            else
                parameters["mbid"] = Artist.MBID;

            var request = new Request<LFSimilarArtistsResponse>("artist.getSimilar", parameters);
            var response = await _lfService.ExecuteRequestAsync(request);

            if (response.IsValid())
            {
                return response.Data.Artists;
            }
            else
                throw new Exception("LFSimilarArtistsResponse isn't valid.");
        }

        private void OnGoToTrackInfoCommand(LFAudioBase audio)
        {
            _navigationService.Navigate("TrackInfoView", JsonConvert.SerializeObject(audio));
        }

        private void OnGoToSimilarArtistInfoCommand(LFSimilarArtist artist)
        {
            if (_purchaseService.IsFullVersionPurchased)
                _navigationService.Navigate("ArtistInfoView", JsonConvert.SerializeObject(artist));
            else
                _navigationService.Navigate("PurchaseView", JsonConvert.SerializeObject(
                    new KeyValuePair<string, string>("ArtistInfoView", JsonConvert.SerializeObject(artist))));
        }

        private void OnGoToAlbumInfoCommand(LFAlbumBase album)
        {
            var parameter = new Tuple<LFAlbumBase, string>(album, Artist.MegaImage.URL);

            if (_purchaseService.IsFullVersionPurchased)
                _navigationService.Navigate("ArtistAlbumView", JsonConvert.SerializeObject(parameter));
            else
                _navigationService.Navigate("PurchaseView", JsonConvert.SerializeObject(
                    new KeyValuePair<string, string>("ArtistAlbumView", JsonConvert.SerializeObject(parameter))));
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

        private readonly ILFService _lfService;
        private readonly INavigationService _navigationService;
        private readonly ISettingsService _settingsService;
        private readonly IPurchaseService _purchaseService;
    }
}
