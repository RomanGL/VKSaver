using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using VKSaver.Core.Models.Database;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Services.Interfaces;
using PropertyChanged;
using Microsoft.Practices.Prism.StoreApps;
using System.Collections.ObjectModel;
using VKSaver.Core.ViewModels.Collections;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class LocalArtistViewModel : AudioViewModel<VKSaverTrack>
    {
        public LocalArtistViewModel(
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
        }

        public string ArtistName { get; private set; }

        public string ArtistImage { get; private set; }

        public SimpleStateSupportCollection<VKSaverTrack> Tracks { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            ArtistName = e.Parameter.ToString();
            Tracks = new SimpleStateSupportCollection<VKSaverTrack>(LoadTracks);
            Tracks.Load();
            LoadArtistImage();

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatingFrom(e, viewModelState, suspending);

            if (e.Cancel)
                return;

            if (!suspending)
                Tracks = null;
        }

        protected override IPlayerTrack ConvertToPlayerTrack(VKSaverTrack track)
        {
            return track;
        }

        protected override IList<VKSaverTrack> GetAudiosList()
        {
            if (Tracks == null)
                return new List<VKSaverTrack>(0);
            return Tracks;
        }

        protected override IList GetSelectionList()
        {
            if (Tracks == null)
                return new List<VKSaverTrack>(0);
            return Tracks;
        }

        private async Task<IEnumerable<VKSaverTrack>> LoadTracks()
        {
            if (Tracks.Count > 0)
                return new List<VKSaverTrack>(0);

            var dbArtist = await _libraryDatabaseService.GetArtist(ArtistName);

            if (dbArtist.Tracks.Any())
                SetDefaultMode();

            return dbArtist.Tracks;
        }

        private async void LoadArtistImage()
        {
            ArtistImage = await _imagesCacheService.GetCachedArtistImage(ArtistName);
            if (ArtistImage == null)
                ArtistImage = await _imagesCacheService.CacheAndGetArtistImage(ArtistName);
        }

        private readonly ILibraryDatabaseService _libraryDatabaseService;
        private readonly IImagesCacheService _imagesCacheService;
    }
}
