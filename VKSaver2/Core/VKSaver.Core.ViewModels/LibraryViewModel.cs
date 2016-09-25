using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Database;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Common;
using Windows.UI.Xaml.Navigation;
using System.Collections;
using VKSaver.Core.Models.Player;
using VKSaver.Core.ViewModels.Collections;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class LibraryViewModel : AudioViewModel<VKSaverTrack>
    {
        public LibraryViewModel(
            INavigationService navigationService,
            ILibraryDatabaseService libraryDatabaseService,
            IPlayerService playerService, 
            ILogService logService,
            IAppLoaderService appLoaderService,
            ILocService locService)
            : base(playerService, locService, navigationService, appLoaderService)
        {
            _libraryDatabaseService = libraryDatabaseService;
            _logService = logService;
        }
                
        public ContentState TracksState { get; private set; }

        [DoNotNotify]
        public int CurrentPivotIndex
        {
            get { return _currentPivotIndex; }
            set
            {
                _currentPivotIndex = value;
                OnPivotIndexChanged(value);
                OnPropertyChanged(nameof(CurrentPivotIndex));
            }
        }

        public SimpleStateSupportCollection<JumpListGroup<VKSaverTrack>> Tracks { get; private set; }
        public SimpleStateSupportCollection<JumpListGroup<VKSaverArtist>> Artists { get; private set; }
        public SimpleStateSupportCollection<JumpListGroup<VKSaverAlbum>> Albums { get; private set; }
        public SimpleStateSupportCollection<JumpListGroup<VKSaverGenre>> Genres { get; private set; }
        public SimpleStateSupportCollection<JumpListGroup<VKSaverTrack>> Cached { get; private set; }
        public SimpleStateSupportCollection<VKSaverFolder> Folders { get; set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (e.NavigationMode == NavigationMode.New || e.NavigationMode == NavigationMode.Refresh)
            {
                Tracks = new SimpleStateSupportCollection<JumpListGroup<VKSaverTrack>>(LoadTracks);
                Artists = new SimpleStateSupportCollection<JumpListGroup<VKSaverArtist>>(LoadArtists);
                Albums = new SimpleStateSupportCollection<JumpListGroup<VKSaverAlbum>>(LoadAlbums);
                Genres = new SimpleStateSupportCollection<JumpListGroup<VKSaverGenre>>(LoadGenres);
                Folders = new SimpleStateSupportCollection<VKSaverFolder>(LoadFolders);
                Cached = new SimpleStateSupportCollection<JumpListGroup<VKSaverTrack>>(LoadCachedTracks);

                string viewName = (string)e.Parameter;
                SetPivotIndex(viewName);
            }
            else if (viewModelState.Count > 0)
            {
                CurrentPivotIndex = (int)viewModelState[nameof(CurrentPivotIndex)];
            }

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatingFrom(e, viewModelState, suspending);

            if (e.Cancel)
                return;

            if (!suspending && e.NavigationMode == NavigationMode.Back)
            {
                Tracks = null;
                Artists = null;
                Albums = null;
                Genres = null;
                Folders = null;
                Cached = null;

                _allTracks = null;
                _allCachedTracks = null;
            }
            else if (e.NavigationMode == NavigationMode.New)
            {
                viewModelState[nameof(CurrentPivotIndex)] = CurrentPivotIndex;
            }
        }

        protected override IList<VKSaverTrack> GetAudiosList()
        {
            if (CurrentPivotIndex == 0)
                return _allTracks ?? new List<VKSaverTrack>(0);
            else if (CurrentPivotIndex == 5)
                return _allCachedTracks ?? new List<VKSaverTrack>(0);
            else
                return new List<VKSaverTrack>();
        }

        protected override IPlayerTrack ConvertToPlayerTrack(VKSaverTrack track)
        {
            return track;
        }

        protected override IList GetSelectionList()
        {
            if (CurrentPivotIndex == 0)
                return _allTracks ?? new List<VKSaverTrack>(0);
            else if (CurrentPivotIndex == 5)
                return _allCachedTracks ?? new List<VKSaverTrack>(0);
            else
                return new List<VKSaverTrack>();
        }

        private void SetPivotIndex(string viewName)
        {            
            switch (viewName)
            {
                case "tracks":
                    CurrentPivotIndex = 0;
                    break;
                case "artists":
                    CurrentPivotIndex = 1;
                    break;
                case "albums":
                    CurrentPivotIndex = 2;
                    break;
                case "genres":
                    CurrentPivotIndex = 3;
                    break;
                case "folders":
                    CurrentPivotIndex = 4;
                    break;
                case "cached":
                    CurrentPivotIndex = 5;
                    break;
                default:
                    CurrentPivotIndex = 0;
                    break;
            }
        }

        private void OnPivotIndexChanged(int newIndex)
        {
            switch (newIndex)
            {
                case 0:
                    Tracks?.Load();
                    break;
                case 1:
                    Artists?.Load();
                    break;
                case 2:
                    Albums?.Load();
                    break;
                case 3:
                    Genres?.Load();
                    break;
                case 4:
                    Folders?.Load();
                    break;
                case 5:
                    Cached?.Load();
                    break;
            }
        }

        private async Task<IEnumerable<JumpListGroup<VKSaverTrack>>> LoadTracks()
        {
            if (Tracks.Count > 0)
            {
                SetDefaultMode();
                return new List<JumpListGroup<VKSaverTrack>>(0);
            }

            var dbTracks = await _libraryDatabaseService.GetAllTracks();
            var groups = dbTracks.ToAlphaGroupsEnumerable(t => t.Title);

            if (groups.Any())
            {
                _allTracks = groups.GroupsToList();
                SetDefaultMode();
            }

            return groups; 
        }

        private async Task<IEnumerable<JumpListGroup<VKSaverArtist>>> LoadArtists()
        {
            HideCommandBar();

            if (Artists.Count > 0)
                return new List<JumpListGroup<VKSaverArtist>>(0);

            var dbArtists = await _libraryDatabaseService.GetAllArtists();
            var groups = dbArtists.ToAlphaGroupsEnumerable(t => t.Name);

            return groups;
        }

        private async Task<IEnumerable<JumpListGroup<VKSaverAlbum>>> LoadAlbums()
        {
            HideCommandBar();

            if (Albums.Count > 0)
                return new List<JumpListGroup<VKSaverAlbum>>(0);

            var dbAlbums = await _libraryDatabaseService.GetAllAlbums();
            var groups = dbAlbums.ToAlphaGroupsEnumerable(t => t.Name);

            return groups;
        }

        private async Task<IEnumerable<JumpListGroup<VKSaverGenre>>> LoadGenres()
        {
            HideCommandBar();

            if (Genres.Count > 0)
                return new List<JumpListGroup<VKSaverGenre>>(0);

            var dbGenres = await _libraryDatabaseService.GetAllGenres();
            var groups = dbGenres.ToAlphaGroupsEnumerable(t => t.Name);

            return groups;
        }

        private async Task<IEnumerable<JumpListGroup<VKSaverTrack>>> LoadCachedTracks()
        {
            if (Cached.Count > 0)
            {
                SetDefaultMode();
                return new List<JumpListGroup<VKSaverTrack>>(0);
            }

            var dbTracks = await _libraryDatabaseService.GetAllCachedTracks();
            var groups = dbTracks.ToAlphaGroupsEnumerable(t => t.Title);

            if (groups.Any())
            {
                _allCachedTracks = groups.GroupsToList();
                SetDefaultMode();
            }

            return groups;
        }

        private async Task<IEnumerable<VKSaverFolder>> LoadFolders()
        {
            HideCommandBar();

            if (Folders.Count > 0)
                return new List<VKSaverFolder>(0);

            return await _libraryDatabaseService.GetAllFolders();
        }

        private int _currentPivotIndex;
        private List<VKSaverTrack> _allTracks;
        private List<VKSaverTrack> _allCachedTracks;
        
        private readonly ILibraryDatabaseService _libraryDatabaseService;
        private readonly ILogService _logService;
    }
}
