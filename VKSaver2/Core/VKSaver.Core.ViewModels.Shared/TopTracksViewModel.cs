#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#else
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#endif

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
using VKSaver.Core.ViewModels.Collections;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class TopTracksViewModel : ViewModelBase
    {
        public TopTracksViewModel(ILFService lfService, INavigationService navigationService)
        {
            _lfService = lfService;
            _navigationService = navigationService;

            GoToTrackInfoCommand = new DelegateCommand<LFAudioBase>(OnGoToTrackInfoCommand);
            Tracks = new PaginatedCollection<LFAudioBase>(LoadMoreTopTracks);
        }

        [DoNotNotify]
        public DelegateCommand<LFAudioBase> GoToTrackInfoCommand { get; private set; }

        [DoNotNotify]
        public PaginatedCollection<LFAudioBase> Tracks { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (Tracks == null && viewModelState.Count > 0)
            {
                var items = JsonConvert.DeserializeObject<List<LFAudioBase>>(
                    viewModelState[nameof(Tracks)].ToString());

                if (items.Count == 0)
                    Tracks = new PaginatedCollection<LFAudioBase>(LoadMoreTopTracks);
                else
                    Tracks = new PaginatedCollection<LFAudioBase>(items, LoadMoreTopTracks) { Page = 1 };
            }
            else if (Tracks == null)
            {
                Tracks = new PaginatedCollection<LFAudioBase>(LoadMoreTopTracks);
            }

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.New)
                viewModelState[nameof(Tracks)] = JsonConvert.SerializeObject(Tracks.Take(50));

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async Task<IEnumerable<LFAudioBase>> LoadMoreTopTracks(uint page)
        {
            var parameters = new Dictionary<string, string>
            {
                { "limit", "25" },
                { "page", (page + 1).ToString() }
            };
            var request = new Request<LFChartTracksResponse>("chart.getTopTracks", parameters);

            var response = await _lfService.ExecuteRequestAsync(request);
            if (response.IsValid())
                return response.Items.Tracks;
            else
                throw new Exception();
        }

        private void OnGoToTrackInfoCommand(LFAudioBase audio)
        {
            _navigationService.Navigate("TrackInfoView", JsonConvert.SerializeObject(audio));
        }

        private readonly ILFService _lfService;
        private readonly INavigationService _navigationService;
    }
}
