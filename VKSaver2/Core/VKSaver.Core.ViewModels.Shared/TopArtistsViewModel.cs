#if WINDOWS_UWP
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
using VKSaver.Core.Services.Json;
using VKSaver.Core.ViewModels.Collections;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Toolkit;
using VKSaver.Core.Toolkit.Navigation;
using NavigatedToEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatedToEventArgs;
using NavigatingFromEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatingFromEventArgs;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class TopArtistsViewModel : VKSaverViewModel
    {
        public TopArtistsViewModel(
            LastfmClient lfClient,
            INavigationService navigationService)
        {
            _lfClient = lfClient;
            _navigationService = navigationService;

            GoToArtistInfoCommand = new DelegateCommand<LastArtist>(OnGoToArtistInfoCommand);
        }

        public PaginatedCollection<LastArtist> Artists { get; private set; }

        [DoNotNotify]
        public DelegateCommand<LastArtist> GoToArtistInfoCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (Artists == null && viewModelState.Count > 0)
            {
                var items = JsonConvert.DeserializeObject<List<LastArtist>>(
                    viewModelState[nameof(Artists)].ToString(), _lastImageSetConverter);

                if (items.Count == 0)
                    Artists = new PaginatedCollection<LastArtist>(LoadMoreArtists);
                else
                    Artists = new PaginatedCollection<LastArtist>(items, LoadMoreArtists) { Page = 1 };
            }
            else if (Artists == null)
            {
                Artists = new PaginatedCollection<LastArtist>(LoadMoreArtists);
            }

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.New)
                viewModelState[nameof(Artists)] = JsonConvert.SerializeObject(Artists.Take(50), _lastImageSetConverter);

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async Task<IEnumerable<LastArtist>> LoadMoreArtists(uint page)
        {
            var response = await _lfClient.Chart.GetTopArtistsAsync((int)(page + 1), 50);
            if (response.Success)
                return response;
            else
                throw new Exception();
        }

        private void OnGoToArtistInfoCommand(LastArtist artist)
        {
            _navigationService.Navigate("ArtistInfoView", JsonConvert.SerializeObject(artist, _lastImageSetConverter));
        }
        
        private readonly LastfmClient _lfClient;
        private readonly INavigationService _navigationService;

        private static readonly LastImageSetConverter _lastImageSetConverter = new LastImageSetConverter();
    }
}
