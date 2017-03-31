#if WINDOWS_UWP || WINDOWS_PHONE_APP
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
#endif

#if WINDOWS_UWP
using Prism.Commands;
using Prism.Windows.Navigation;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#elif ANDROID
using VKSaver.Core.Toolkit.Commands;
using Android.Widget;
using Android.Views.InputMethods;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using Newtonsoft.Json;
using PropertyChanged;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Services.Json;
using VKSaver.Core.Toolkit.Controls;
using VKSaver.Core.Toolkit.Navigation;
using VKSaver.Core.ViewModels.Collections;
using VKSaver.Core.ViewModels.Search;
using NavigatedToEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatedToEventArgs;
using NavigatingFromEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatingFromEventArgs;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class ArtistsSearchViewModel : WithAppBarViewModel
    {
        public ArtistsSearchViewModel(
            LastfmClient lastfmClient, 
            ILocService locService,
            INavigationService navigationService)
        {
            _lastfmClient = lastfmClient;
            _locService = locService;
            _navigationService = navigationService;
            
            GoToArtistInfoCommand = new DelegateCommand<LastArtist>(OnGoToArtistInfoCommand);
            ReloadCommand = new DelegateCommand(Search);

#if WINDOWS_UWP || WINDOWS_PHONE_APP
            QueryBoxKeyDownCommand = new DelegateCommand<KeyRoutedEventArgs>(OnQueryBoxKeyDownCommand);
#elif ANDROID
            QueryBoxActionCommand = new DelegateCommand<TextView.EditorActionEventArgs>(OnQueryBoxActionCommand);
#endif
        }
        
        public string Query { get; set; }
        public PaginatedCollection<LastArtist> Artists { get; private set; }

#if WINDOWS_UWP || WINDOWS_PHONE_APP
        [DoNotNotify]
        public DelegateCommand<KeyRoutedEventArgs> QueryBoxKeyDownCommand { get; private set; }
#elif ANDROID
        [DoNotNotify]
        public DelegateCommand<TextView.EditorActionEventArgs> QueryBoxActionCommand { get; private set; }
#endif

        [DoNotNotify]
        public DelegateCommand<LastArtist> GoToArtistInfoCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand ReloadCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (viewModelState.Count == 0)
            {
                if (e.Parameter != null)
                {
                    var parameter = JsonConvert.DeserializeObject<SearchNavigationParameter>(e.Parameter.ToString());
                    Query = parameter.Query;
                }
                
                Artists = new PaginatedCollection<LastArtist>(LoadMoreArtists);

                Artists.HasMoreItems = false;
                Artists.ContentState = ContentState.NoData;

                if (!String.IsNullOrWhiteSpace(Query))
                    Search();
                else
                    HideCommandBar();
            }
            else
            {
                Query = (string)viewModelState[nameof(Query)];
                _currentQuery = (string)viewModelState[nameof(_currentQuery)];

                Artists = JsonConvert.DeserializeObject<PaginatedCollection<LastArtist>>(
                    viewModelState[nameof(Artists)].ToString(), _lastImageSetConverter);

                Artists.LoadMoreItems = LoadMoreArtists;

                Artists.Page = (uint)viewModelState[nameof(Artists) + "PageOffset"];
                Artists.ContentState = (ContentState)(int)viewModelState[nameof(Artists) + "State"];

                if (Artists.ContentState == ContentState.NoData)
                    Artists.HasMoreItems = false;

                if (!Artists.Any() && String.IsNullOrEmpty(Query))
                    HideCommandBar();
                else
                    SetDefaultMode();
            }

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                viewModelState[nameof(Query)] = Query;
                viewModelState[nameof(_currentQuery)] = _currentQuery;
                viewModelState[nameof(Artists) + "PageOffset"] = Artists.Page;
                viewModelState[nameof(Artists)] = JsonConvert.SerializeObject(Artists.ToList(), _lastImageSetConverter);
                viewModelState[nameof(Artists) + "State"] = (int)Artists.ContentState;
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        protected override void CreateDefaultAppBarButtons()
        {
            AppBarItems.Add(new ButtonElement
            {
                Label = _locService["AppBarButton_Refresh_Text"],
                Icon = new FontButtonIcon { Glyph = "\uE117", FontSize = 14 },
                Command = ReloadCommand
            });

            base.CreateDefaultAppBarButtons();
        }

        private async Task<IEnumerable<LastArtist>> LoadMoreArtists(uint page)
        {
            var response = await _lastfmClient.Artist.SearchAsync(_currentQuery, (int)(page + 1), 50);
            if (response.Success)
            {
                if (!Artists.Any())
                    SetDefaultMode();
                return response;
            }
            else
                throw new Exception();
        }

        private void OnGoToArtistInfoCommand(LastArtist artist)
        {
            _navigationService.Navigate("ArtistInfoView", JsonConvert.SerializeObject(artist, _lastImageSetConverter));
        }

#if WINDOWS_UWP || WINDOWS_PHONE_APP
        private void OnQueryBoxKeyDownCommand(KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
                Search();
        }
#elif ANDROID
        private void OnQueryBoxActionCommand(TextView.EditorActionEventArgs e)
        {
            if (e.ActionId == ImeAction.Search)
                Search();
        }
#endif

        private void Search()
        {
            HideCommandBar();
            _currentQuery = Query;
            Artists.Refresh();
        }

        private string _currentQuery;

        private readonly ILocService _locService;
        private readonly LastfmClient _lastfmClient;
        private readonly INavigationService _navigationService;

        private static readonly LastImageSetConverter _lastImageSetConverter = new LastImageSetConverter();
    }
}
