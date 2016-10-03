#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#else
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#endif

using PropertyChanged;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VKSaver.Core.Models.Database;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Collections;
using Windows.Storage;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class LocalAlbumViewModel : AudioViewModel<VKSaverTrack>
    {
        public LocalAlbumViewModel(
            IPlayerService playerService,
            ILocService locService,
            INavigationService navigationService,
            IAppLoaderService appLoaderService,
            ILibraryDatabaseService libraryDatabaseService,
            IImagesCacheService imagesCacheService) 
            : base(playerService, locService, navigationService, appLoaderService)
        {
            _libraryDatabaseService = libraryDatabaseService;
            _imagesCacheService = imagesCacheService;

            DeleteItemCommand = new DelegateCommand<object>(OnDeleteItemCommand);
        }

        public string AlbumName { get; private set; }

        public string ArtistName { get; private set; }

        public string ArtistImage { get; private set; }

        public SimpleStateSupportCollection<VKSaverTrack> Tracks { get; private set; }

        [DoNotNotify]
        public DelegateCommand<object> DeleteItemCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            _albumDbKey = e.Parameter.ToString();
            Tracks = new SimpleStateSupportCollection<VKSaverTrack>(LoadTracks);
            Tracks.Load();

            base.OnNavigatedTo(e, viewModelState);
        }

        protected override IPlayerTrack ConvertToPlayerTrack(VKSaverTrack track)
        {
            return track;
        }

        protected override IList<VKSaverTrack> GetAudiosList()
        {
            return Tracks;
        }

        protected override IList GetSelectionList()
        {
            return Tracks;
        }

        private async Task<IEnumerable<VKSaverTrack>> LoadTracks()
        {
            if (Tracks.Count > 0)
                return new List<VKSaverTrack>(0);

            var dbAlbum = await _libraryDatabaseService.GetAlbum(_albumDbKey);

            if (dbAlbum.Tracks.Any())
                SetDefaultMode();

            AlbumName = dbAlbum.Name == LibraryDatabaseService.UNKNOWN_ALBUM_NAME ? 
                _locService["UnknownAlbum_Text"] : 
                dbAlbum.Name;
            ArtistName = dbAlbum.ArtistName == LibraryDatabaseService.UNKNOWN_ARTIST_NAME ?
                _locService["UnknownArtist_Text"] :
                dbAlbum.ArtistName;

            foreach (var track in dbAlbum.Tracks)
            {
                if (track.Artist == LibraryDatabaseService.UNKNOWN_ARTIST_NAME)
                    track.Artist = _locService["UnknownArtist_Text"];
            }

            LoadArtistImage(dbAlbum.ArtistName);

            return dbAlbum.Tracks;
        }

        private async void LoadArtistImage(string artistName)
        {
            ArtistImage = await _imagesCacheService.GetCachedArtistImage(artistName);
            if (ArtistImage == null)
            {
                ArtistImage = AppConstants.DEFAULT_PLAYER_BACKGROUND_IMAGE;
                var img = await _imagesCacheService.CacheAndGetArtistImage(artistName);
                if (img != null)
                    ArtistImage = img;
            }
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
                    Tracks.Remove((VKSaverTrack)item);

                    var file = await MusicFilesPathHelper.GetFileFromCapatibleName(track.Source);
                    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
                else if (item is VKSaverAlbum)
                {
                    if (((VKSaverAlbum)item).DbKey == track.AlbumKey)
                        _navigationService.GoBack();
                }
            }
            _appLoaderService.Hide();
        }

        private string _albumDbKey;

        private readonly ILibraryDatabaseService _libraryDatabaseService;
        private readonly IImagesCacheService _imagesCacheService;
    }
}
