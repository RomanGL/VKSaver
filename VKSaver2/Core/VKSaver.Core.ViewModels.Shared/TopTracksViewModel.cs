#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#else
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#endif

using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VKSaver.Core.Services.Json;
using VKSaver.Core.ViewModels.Collections;
using Windows.UI.Xaml.Navigation;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class TopTracksViewModel : ViewModelBase
    {
        public TopTracksViewModel(LastfmClient lfClient, INavigationService navigationService)
        {
            _lfClient = lfClient;
            _navigationService = navigationService;

            GoToTrackInfoCommand = new DelegateCommand<LastTrack>(OnGoToTrackInfoCommand);
            Tracks = new PaginatedCollection<LastTrack>(LoadMoreTopTracks);
        }

        [DoNotNotify]
        public DelegateCommand<LastTrack> GoToTrackInfoCommand { get; private set; }

        [DoNotNotify]
        public PaginatedCollection<LastTrack> Tracks { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (Tracks == null && viewModelState.Count > 0)
            {
                var items = JsonConvert.DeserializeObject<List<LastTrack>>(
                    viewModelState[nameof(Tracks)].ToString(), _lastImageSetConverter);

                if (items.Count == 0)
                    Tracks = new PaginatedCollection<LastTrack>(LoadMoreTopTracks);
                else
                    Tracks = new PaginatedCollection<LastTrack>(items, LoadMoreTopTracks) { Page = 1 };
            }
            else if (Tracks == null)
            {
                Tracks = new PaginatedCollection<LastTrack>(LoadMoreTopTracks);
            }

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.New)
                viewModelState[nameof(Tracks)] = JsonConvert.SerializeObject(Tracks.Take(50), _lastImageSetConverter);

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async Task<IEnumerable<LastTrack>> LoadMoreTopTracks(uint page)
        {
            var response = await _lfClient.Chart.GetTopTracksAsync(((int)page + 1), 25);
            if (response.Success)
                return response;
            else
                throw new Exception();
        }

        private void OnGoToTrackInfoCommand(LastTrack audio)
        {
            _navigationService.Navigate("TrackInfoView", JsonConvert.SerializeObject(audio, _lastImageSetConverter));
        }
        
        private readonly INavigationService _navigationService;
        private readonly LastfmClient _lfClient;

        private readonly static LastImageSetConverter _lastImageSetConverter = new LastImageSetConverter();
    }
}
