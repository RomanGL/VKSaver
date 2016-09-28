using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
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
    public sealed class LocalGenreViewModel : AudioViewModel<VKSaverTrack>
    {
        public LocalGenreViewModel(
            IPlayerService playerService,
            ILocService locService,
            INavigationService navigationService,
            IAppLoaderService appLoaderService,
            ILibraryDatabaseService libraryDatabaseService) 
            : base(playerService, locService, navigationService, appLoaderService)
        {
            _libraryDatabaseService = libraryDatabaseService;

            DeleteItemCommand = new DelegateCommand<object>(OnDeleteItemCommand);
        }

        public string GenreName { get; private set; }

        public SimpleStateSupportCollection<VKSaverTrack> Tracks { get; private set; }

        [DoNotNotify]
        public DelegateCommand<object> DeleteItemCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            _genreKey = e.Parameter.ToString();
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

            var dbGenre = await _libraryDatabaseService.GetGenre(_genreKey);

            GenreName = dbGenre.Name == LibraryDatabaseService.UNKNOWN_GENRE_NAME ?
                _locService["UnknownGenre_Text"] :
                dbGenre.Name;

            if (dbGenre.Tracks.Any())
                SetDefaultMode();

            return dbGenre.Tracks;
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
                else if (item is VKSaverGenre)
                {
                    if (((VKSaverGenre)item).DbKey == track.GenreKey)
                        _navigationService.GoBack();
                }
            }

            _appLoaderService.Hide();
        }

        private string _genreKey;

        private readonly ILibraryDatabaseService _libraryDatabaseService;
    }
}
