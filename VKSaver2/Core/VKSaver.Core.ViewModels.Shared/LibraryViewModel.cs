#if WINDOWS_UWP
using Prism.Commands;
using Prism.Windows.Navigation;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#elif ANDROID
#endif

using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Database;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Common;
using Windows.UI.Xaml.Navigation;
using System.Collections;
using VKSaver.Core.Models.Player;
using VKSaver.Core.ViewModels.Collections;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services;

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
            ILocService locService,
            IDialogsService dialogsService)
            : base(playerService, locService, navigationService, appLoaderService)
        {
            _libraryDatabaseService = libraryDatabaseService;
            _logService = logService;
            _dialogsService = dialogsService;

            ExecuteItemCommand = new DelegateCommand<object>(OnExecuteItemCommand);
            DeleteItemCommand = new DelegateCommand<object>(OnDeleteItemCommand);
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

        [DoNotNotify]
        public DelegateCommand<object> ExecuteItemCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<object> DeleteItemCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            Tracks = Tracks ?? new SimpleStateSupportCollection<JumpListGroup<VKSaverTrack>>(LoadTracks);
            Artists = Artists ?? new SimpleStateSupportCollection<JumpListGroup<VKSaverArtist>>(LoadArtists);
            Albums = Albums ?? new SimpleStateSupportCollection<JumpListGroup<VKSaverAlbum>>(LoadAlbums);
            Genres = Genres ?? new SimpleStateSupportCollection<JumpListGroup<VKSaverGenre>>(LoadGenres);
            Folders = Folders ?? new SimpleStateSupportCollection<VKSaverFolder>(LoadFolders);
            Cached = Cached ?? new SimpleStateSupportCollection<JumpListGroup<VKSaverTrack>>(LoadCachedTracks);

            if (_libraryDatabaseService.NeedReloadLibraryView)
            {
                Tracks.Clear();
                Artists.Clear();
                Albums.Clear();
                Genres.Clear();
                Folders.Clear();
                Cached.Clear();

                _libraryDatabaseService.NeedReloadLibraryView = false;
            }

            if (viewModelState.Count > 0)
            {
                CurrentPivotIndex = (int)viewModelState[nameof(CurrentPivotIndex)];
            }
            else
            {
                string viewName = (string)e.Parameter;
                SetPivotIndex(viewName);
            }

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatingFrom(e, viewModelState, suspending);

            if (e.Cancel)
                return;

            if (e.NavigationMode == NavigationMode.Back)
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
            if (!dbTracks.Any())
            {
                HideCommandBar();
                return new List<JumpListGroup<VKSaverTrack>>(0);
            }

            foreach (var track in dbTracks)
            {
                if (track.Artist == LibraryDatabaseService.UNKNOWN_ARTIST_NAME)
                    track.Artist = _locService["UnknownArtist_Text"];
            }

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
            if (!dbArtists.Any())
                return new List<JumpListGroup<VKSaverArtist>>(0);

            foreach (var artist in dbArtists)
            {
                if (artist.Name == LibraryDatabaseService.UNKNOWN_ARTIST_NAME)
                    artist.Name = _locService["UnknownArtist_Text"];
            }

            var groups = dbArtists.ToAlphaGroupsEnumerable(t => t.Name);

            return groups;
        }

        private async Task<IEnumerable<JumpListGroup<VKSaverAlbum>>> LoadAlbums()
        {
            HideCommandBar();

            if (Albums.Count > 0)
                return new List<JumpListGroup<VKSaverAlbum>>(0);

            var dbAlbums = await _libraryDatabaseService.GetAllAlbums();
            if (!dbAlbums.Any())
                return new List<JumpListGroup<VKSaverAlbum>>(0);

            foreach (var album in dbAlbums)
            {
                if (album.Name == LibraryDatabaseService.UNKNOWN_ALBUM_NAME)
                    album.Name = _locService["UnknownAlbum_Text"];
                if (album.ArtistName == LibraryDatabaseService.UNKNOWN_ARTIST_NAME)
                    album.ArtistName = _locService["UnknownArtist_Text"];
            }

            var groups = dbAlbums.ToAlphaGroupsEnumerable(t => t.Name);

            return groups;
        }
        
        private async Task<IEnumerable<JumpListGroup<VKSaverGenre>>> LoadGenres()
        {
            HideCommandBar();

            if (Genres.Count > 0)
                return new List<JumpListGroup<VKSaverGenre>>(0);

            var dbGenres = await _libraryDatabaseService.GetAllGenres();
            if (!dbGenres.Any())
                return new List<JumpListGroup<VKSaverGenre>>(0);

            foreach (var genre in dbGenres)
            {
                if (genre.Name == LibraryDatabaseService.UNKNOWN_GENRE_NAME)
                    genre.Name = _locService["UnknownGenre_Text"];
            }

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
            if (!dbTracks.Any())
            {
                HideCommandBar();
                return new List<JumpListGroup<VKSaverTrack>>(0);
            }

            foreach (var track in dbTracks)
            {
                if (track.Artist == LibraryDatabaseService.UNKNOWN_ARTIST_NAME)
                    track.Artist = _locService["UnknownArtist_Text"];
            }

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

        private void OnExecuteItemCommand(object item)
        {
            if (item is VKSaverArtist)
                _navigationService.Navigate("LocalArtistView", ((VKSaverArtist)item).DbKey);
            else if (item is VKSaverAlbum)
                _navigationService.Navigate("LocalAlbumView", ((VKSaverAlbum)item).DbKey);
            else if (item is VKSaverGenre)
                _navigationService.Navigate("LocalGenreView", ((VKSaverGenre)item).DbKey);
            else if (item is VKSaverFolder)
                _navigationService.Navigate("LocalFolderView", ((VKSaverFolder)item).Path);
        }

        private async void OnDeleteItemCommand(object item)
        {
            if (item is VKSaverTrack)
            {
                await DeleteTrack((VKSaverTrack)item);
            }          
        }

        private async Task DeleteTrack(VKSaverTrack track)
        {
            _appLoaderService.Show(String.Format(_locService["AppLoader_DeletingItem"], track.Title));

            var cleaner = _libraryDatabaseService.GetCleaner();
            var result = await cleaner.RemoveItemAndCleanDependenciesAsync(track);            

            foreach (var item in result)
            {
                if (item is VKSaverTrack)
                {
                    var itemTrack = (VKSaverTrack)item;

                    _allTracks?.Remove(itemTrack);
                    DeleteItemFromGroups(itemTrack, Tracks);

                    if (itemTrack.VKInfoKey != null)
                    {
                        _allCachedTracks?.Remove(itemTrack);
                        DeleteItemFromGroups(itemTrack, Cached);
                    }

                    var file = await MusicFilesPathHelper.GetFileFromCapatibleName(track.Source);
                    await file.DeleteAsync();
                }
                else if (item is VKSaverArtist)
                {
                    DeleteItemFromGroups((VKSaverArtist)item, Artists);
                }
                else if (item is VKSaverGenre)
                {
                    DeleteItemFromGroups((VKSaverGenre)item, Genres);
                }
                else if (item is VKSaverAlbum)
                {
                    DeleteItemFromGroups((VKSaverAlbum)item, Albums);
                }
                else if (item is VKSaverFolder)
                {
                    Folders.Remove((VKSaverFolder)item);
                }
            }

            _libraryDatabaseService.NeedReloadLibraryView = false;
            _appLoaderService.Hide();
        }

        private async Task DeleteArtist(VKSaverArtist artist)
        {
            _appLoaderService.Show(String.Format(_locService["AppLoader_DeletingItem"], artist.Name));

            bool canDelete = await _dialogsService.ShowYesNoAsync(
                String.Format(_locService["Message_LocalView_DeleteArtist_Text"], artist.Name), 
                _locService["Message_LocalView_DeleteArtist_Title"]);

            if (!canDelete)
            {
                _appLoaderService.Hide();
                return;
            }

            var result = await _libraryDatabaseService.GetCleaner().RemoveItemAndCleanDependenciesAsync(artist);
            //var dbArtist = await _libraryDatabaseService.GetArtist(artist.DbKey);

            //foreach (var track in dbArtist.Tracks)
            //{
            //    DeleteItemFromGroups(track, Tracks);
            //    if (track.VKInfoKey != null)
            //    {
            //        await _libraryDatabaseService.RemoveItemByPrimaryKey<VKSaverAudioVKInfo>(track.VKInfoKey);
            //        DeleteItemFromGroups(track, Cached);
            //    }

            //    var genre = await _libraryDatabaseService.GetGenre(track.GenreKey);
            //    if (genre != null && genre.Tracks.Count == 1)
            //    {
            //        await _libraryDatabaseService.RemoveItem(genre);
            //        DeleteItemFromGroups(genre, Genres);
            //    }

            //    await _libraryDatabaseService.RemoveItem(track);
            //}

            //foreach (var album in dbArtist.Albums)
            //{
            //    DeleteItemFromGroups(album, Albums);
            //    await _libraryDatabaseService.RemoveItem(album);
            //}

            //await _libraryDatabaseService.RemoveItem(artist);
            //DeleteItemFromGroups(artist, Artists);

            _libraryDatabaseService.NeedReloadLibraryView = false;
            _appLoaderService.Hide();
        }

        private void DeleteItemFromGroups<T>(T item, IList<JumpListGroup<T>> collection)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                if (collection[i].Remove(item))
                    return;
            }
        }

        private int _currentPivotIndex;
        private List<VKSaverTrack> _allTracks;
        private List<VKSaverTrack> _allCachedTracks;
        
        private readonly ILibraryDatabaseService _libraryDatabaseService;
        private readonly ILogService _logService;
        private readonly IDialogsService _dialogsService;
    }
}
