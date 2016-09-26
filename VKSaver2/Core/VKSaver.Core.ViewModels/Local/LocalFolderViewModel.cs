using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using PropertyChanged;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VKSaver.Core.Models.Database;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Collections;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class LocalFolderViewModel : AudioViewModel<VKSaverTrack>
    {
        public LocalFolderViewModel(
            IPlayerService playerService,
            ILocService locService,
            INavigationService navigationService,
            IAppLoaderService appLoaderService,
            ILibraryDatabaseService libraryDatabaseService) 
            : base(playerService, locService, navigationService, appLoaderService)
        {
            _libraryDatabaseService = libraryDatabaseService;
        }

        public string FolderName { get; private set; }

        public SimpleStateSupportCollection<VKSaverTrack> Tracks { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            _folderDbKey = e.Parameter.ToString();
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

            var dbFolder = await _libraryDatabaseService.GetFolder(_folderDbKey);
            FolderName = dbFolder.Name;

            if (dbFolder.Tracks.Any())
                SetDefaultMode();

            return dbFolder.Tracks;
        }

        private string _folderDbKey;

        private readonly ILibraryDatabaseService _libraryDatabaseService;
    }
}
