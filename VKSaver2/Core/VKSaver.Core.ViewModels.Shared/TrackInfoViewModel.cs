#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#else
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#endif

using ModernDev.InTouch;
using Newtonsoft.Json;
using OneTeam.SDK.LastFm.Models.Audio;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Collections;
using VKSaver.Core.ViewModels.Common;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class TrackInfoViewModel : ViewModelBase
    {
        public TrackInfoViewModel(
            INavigationService navigationService, 
            InTouch inTouch,
            IInTouchWrapper inTouchWrapper, 
            IPlayerService playerService,
            IDownloadsServiceHelper downloadsServiceHelper, 
            IAppLoaderService appLoaderService,
            IDialogsService dialogsService, 
            ILocService locService,
            IImagesCacheService imagesCacheService)
        {
            _navigationService = navigationService;
            _inTouch = inTouch;
            _inTouchWrapper = inTouchWrapper;
            _playerService = playerService;
            _downloadsServiceHelper = downloadsServiceHelper;
            _appLoaderService = appLoaderService;
            _dialogsService = dialogsService;
            _locService = locService;
            _imagesCacheService = imagesCacheService;

            ShowOtherTracksCommand = new DelegateCommand(OnShowOtherTracksCommand);
            PlayTracksCommand = new DelegateCommand<Audio>(OnPlayTracksCommand);
            DownloadTrackCommand = new DelegateCommand<Audio>(OnDownloadTrackCommand);
            AddToMyAudiosCommand = new DelegateCommand<Audio>(OnAddToMyAudiosCommand);
        }

        public string ArtistImage { get; private set; }

        public LFAudioBase Track { get; private set; }

        [DoNotNotify]
        public DelegateCommand ShowOtherTracksCommand { get; private set; }

        [DoNotNotify]
        public SimpleStateSupportCollection<Audio> VKTracks { get; private set; }

        [DoNotNotify]
        public DelegateCommand<Audio> PlayTracksCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<Audio> DownloadTrackCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<Audio> AddToMyAudiosCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            Track = JsonConvert.DeserializeObject<LFAudioBase>(e.Parameter.ToString());

            if (viewModelState.Count == 0)
            {
                VKTracks = new SimpleStateSupportCollection<Audio>(LoadVKTracks);
                VKTracks.Load();
            }
            else
            {
                VKTracks = JsonConvert.DeserializeObject<SimpleStateSupportCollection<Audio>>(
                    viewModelState[nameof(VKTracks)].ToString());
            }

            LoadArtistImage(Track.Artist.Name);
            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (!suspending && e.NavigationMode == NavigationMode.New)
            {
                if (VKTracks != null && VKTracks.Count > 0)
                    viewModelState[nameof(VKTracks)] = JsonConvert.SerializeObject(VKTracks);
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async Task<IEnumerable<Audio>> LoadVKTracks()
        {
            if (Track == null || Track.Artist == null)
                return new List<Audio>(0);

            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.Search(new AudioSearchParams
            {
                AutoComplete = true,
                Count = 10,
                Query = $"{Track.Artist.Name} - {Track.Name}"
            }));

            if (response.IsError)
                throw new Exception(response.Error.ToString());

            return response.Data.Items;
        }

        private async void OnPlayTracksCommand(Audio track)
        {
            _appLoaderService.Show();

            await _playerService.PlayNewTracks(VKTracks.ToPlayerTracks(), VKTracks.IndexOf(track));
            _navigationService.Navigate("PlayerView", null);

            _appLoaderService.Hide();
        }

        private void OnShowOtherTracksCommand()
        {
            _navigationService.Navigate("AccessDeniedView", null);
        }

        private async void OnDownloadTrackCommand(Audio track)
        {
            await _downloadsServiceHelper.StartDownloadingAsync(track.ToDownloadable());
        }

        private async void OnAddToMyAudiosCommand(Audio track)
        {
            _appLoaderService.Show(String.Format(_locService["AppLoader_AddingItem"], track.ToString()));

            bool isSuccess = false;
            try
            {
                isSuccess = await AddToMyAudios(track);
            }
            catch (Exception) { }

            if (!isSuccess)
            {
                _dialogsService.Show(_locService["Message_AudioAddError_Text"],
                    _locService["Message_AudioAddError_Title"]);
            }
            _appLoaderService.Hide();
        }

        private async Task<bool> AddToMyAudios(Audio audio)
        {
            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.Add(
                audio.Id, audio.OwnerId));

            return !response.IsError;
        }

        private async void LoadArtistImage(string artist)
        {
            string img = await _imagesCacheService.GetCachedArtistImage(artist);
            if (img == null)
            {
                ArtistImage = AppConstants.DEFAULT_PLAYER_BACKGROUND_IMAGE;
                img = await _imagesCacheService.CacheAndGetArtistImage(artist);

                if (img != null && artist == Track?.Artist?.Name)
                    ArtistImage = img;
            }
            else if (artist == Track?.Artist?.Name)
            {
                ArtistImage = img;
            }
        }

        private readonly INavigationService _navigationService;
        private readonly InTouch _inTouch;
        private readonly IInTouchWrapper _inTouchWrapper;
        private readonly IPlayerService _playerService;
        private readonly IDownloadsServiceHelper _downloadsServiceHelper;
        private readonly IAppLoaderService _appLoaderService;
        private readonly IDialogsService _dialogsService;
        private readonly ILocService _locService;
        private readonly IImagesCacheService _imagesCacheService;
    }
}
