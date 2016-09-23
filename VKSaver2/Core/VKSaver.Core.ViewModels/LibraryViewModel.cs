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
        
        public ObservableCollection<JumpListGroup<VKSaverTrack>> Tracks { get; private set; }

        [DoNotNotify]
        public DelegateCommand ReloadDataCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (e.NavigationMode == NavigationMode.New || e.NavigationMode == NavigationMode.Refresh)
            {
                Tracks = null;
            }

            LoadData();
            base.OnNavigatedTo(e, viewModelState);
        }
        
        protected override IList<VKSaverTrack> GetAudiosList()
        {
            return _allTracks;
        }

        protected override IPlayerTrack ConvertToPlayerTrack(VKSaverTrack track)
        {
            return track;
        }

        protected override IList GetSelectionList()
        {
            return _allTracks;
        }

        private async void LoadData()
        {
            if (Tracks != null)
                return;

            TracksState = ContentState.Loading;

            try
            {
                _allTracks = await _libraryDatabaseService.GetAllTracks();
                Tracks = _allTracks.ToAlphaGroups(t => t.Title);

                if (Tracks.Any())
                    TracksState = ContentState.Normal;
                else
                    TracksState = ContentState.NoData;
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
                TracksState = ContentState.Error;
            }
        }

        private List<VKSaverTrack> _allTracks = new List<VKSaverTrack>();
        
        private readonly ILibraryDatabaseService _libraryDatabaseService;
        private readonly ILogService _logService;
    }
}
