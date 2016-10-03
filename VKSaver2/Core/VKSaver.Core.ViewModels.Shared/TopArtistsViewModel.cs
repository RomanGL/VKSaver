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
    public sealed class TopArtistsViewModel : ViewModelBase
    {
        public TopArtistsViewModel(ILFService lfService, INavigationService navigationService)
        {
            _lfService = lfService;
            _navigationService = navigationService;

            GoToArtistInfoCommand = new DelegateCommand<LFArtistExtended>(OnGoToArtistInfoCommand);
        }

        public PaginatedCollection<LFArtistExtended> Artists { get; private set; }

        [DoNotNotify]
        public DelegateCommand<LFArtistExtended> GoToArtistInfoCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (Artists == null && viewModelState.Count > 0)
            {
                var items = JsonConvert.DeserializeObject<List<LFArtistExtended>>(
                    viewModelState[nameof(Artists)].ToString());

                if (items.Count == 0)
                    Artists = new PaginatedCollection<LFArtistExtended>(LoadMoreArtists);
                else
                    Artists = new PaginatedCollection<LFArtistExtended>(items, LoadMoreArtists) { Page = 1 };
            }
            else if (Artists == null)
            {
                Artists = new PaginatedCollection<LFArtistExtended>(LoadMoreArtists);
            }

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.New)
                viewModelState[nameof(Artists)] = JsonConvert.SerializeObject(Artists.Take(50));

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async Task<IEnumerable<LFArtistExtended>> LoadMoreArtists(uint page)
        {
            var parameters = new Dictionary<string, string>
            {
                { "page", (page + 1).ToString() },
                { "limit", "50" }
            };

            var request = new Request<LFChartArtistsResponse>("chart.getTopArtists", parameters);
            var response = await _lfService.ExecuteRequestAsync(request);

            if (response.IsValid())
                return response.Data.Artists;
            else
                throw new Exception();
        }

        private void OnGoToArtistInfoCommand(LFArtistExtended artist)
        {
            _navigationService.Navigate("ArtistInfoView", JsonConvert.SerializeObject(artist));
        }

        private readonly ILFService _lfService;
        private readonly INavigationService _navigationService;
    }
}
