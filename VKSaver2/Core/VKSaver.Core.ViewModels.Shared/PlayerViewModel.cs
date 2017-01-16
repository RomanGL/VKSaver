#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#else
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#endif

using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using ModernDev.InTouch;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class PlayerViewModel : VKAudioViewModel<PlayerViewModel.PlayerItem>
    {
        public event TypedEventHandler<PlayerViewModel, PlayerItem> TrackChanged;

        public PlayerViewModel(
            InTouch inTouch,
            IInTouchWrapper inTouchWrapper,
            INavigationService navigationService, 
            IPlayerService playerService,
            IPlayerPlaylistService playerPlaylistService, 
            IImagesCacheService imagesCacheService,
            ITracksShuffleService tracksShuffleService, 
            IDownloadsServiceHelper downloadsServiceHelper,
            IAppLoaderService appLoaderService, 
            ILastFmLoginService lastFmLoginService,
            IPurchaseService purchaseService,
            ILocService locService,
            IDialogsService dialogsService)
            : base(inTouch, appLoaderService, dialogsService, inTouchWrapper, downloadsServiceHelper, playerService, locService, navigationService)
        {
            IsReloadButtonSupported = false;
            IsShuffleButtonSupported = false;
            IsPlayButtonSupported = false;

            _playerPlaylistService = playerPlaylistService;
            _imagesCacheService = imagesCacheService;
            _tracksShuffleSevice = tracksShuffleService;
            _lastFmLoginService = lastFmLoginService;
            _purchaseService = purchaseService;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };

            NextTrackCommand = new DelegateCommand(OnNextTrackCommand);
            PreviousTrackCommand = new DelegateCommand(OnPreviosTrackCommand);
            PlayPauseCommand = new DelegateCommand(OnPlayPauseCommand);
            PlayTrackCommand = new DelegateCommand<PlayerItem>(OnPlayTrackCommand);
            ShowLyricsCommand = new DelegateCommand(OnShowLyricsCommand,
                () => CurrentTrack?.Track.VKInfo != null && CurrentTrack.Track.VKInfo.LyricsID != 0);
        }

        public PlayerItem CurrentTrack { get; private set; }

        public ImageSource ArtistImage { get; private set; }

        public ImageSource TrackImage { get; private set; }

        public ObservableCollection<PlayerItem> Tracks { get; private set; }

        public bool IsLockedPivot { get; private set; }

        public bool IsScrobbleMode
        {
            get { return _isScrobbleMode; }
            set
            {
                if (!value)
                {
                    _isScrobbleMode = false;
                    _playerService.IsScrobbleMode = false;
                    return;
                }

                TryEnableScrobbleMode();
            }
        }

        public bool IsShuffleMode
        {
            get { return _isShuffleMode; }
            set
            {
                _isShuffleMode = value;
                ShuffleTracks(value);
            }
        }

        public bool CanShuffle { get; private set; }

        public PlayerRepeatMode RepeatMode
        {
            get { return _repeatMode; }
            set
            {
                _repeatMode = value;
                _playerService.RepeatMode = value;
            }
        }

        public TimeSpan Duration { get; private set; }

        public TimeSpan Position
        {
            get { return _position; }
            set
            {
                if (_noPositionUpdates)
                {
                    _noPositionUpdates = false;
                    return;
                }

                _position = value;
                _playerService.Position = value;
            }
        }

        public bool IsPlaying { get; set; }

        public int CurrentPivotIndex
        {
            get { return _currentPivotIndex; }
            set
            {
                _currentPivotIndex = value;
                OnPropertyChanged(nameof(CurrentPivotIndex));

                UpdateAppBarState(value);
            }
        }

        [DoNotNotify]
        public DelegateCommand NextTrackCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand PreviousTrackCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand PlayPauseCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<PlayerItem> PlayTrackCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand ShowLyricsCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            _noPositionUpdates = true;

            if (!_isSubscribed)
            {
                _playerService.TrackChanged += PlayerService_TrackChanged;
                _playerService.PlayerStateChanged += PlayerService_PlayerStateChanged;
                _timer.Tick += Timer_Tick;

                _timer.Start();
                _isSubscribed = true;
            }

            LoadPlayerState();
            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (!suspending && _isSubscribed)
            {
                _playerService.TrackChanged -= PlayerService_TrackChanged;
                _playerService.PlayerStateChanged -= PlayerService_PlayerStateChanged;
                _timer.Tick -= Timer_Tick;

                _timer.Stop();
                _isSubscribed = false;

                AppBarItems.Clear();
                SecondaryItems.Clear();
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        protected override IList GetSelectionList() => Tracks;
        protected override IList<PlayerItem> GetAudiosList() => Tracks;
        protected override IPlayerTrack ConvertToPlayerTrack(PlayerItem track) => track.Track;
        protected override IDownloadable ConvertToDownloadable(PlayerItem track) => track.Track as IDownloadable;
        
        protected override string TrackToFriendlyNameString(PlayerItem track)
        {
            return $"{track.Track.Artist} - {track.Track.Title}";
        }

        protected override bool CanDownloadTrack(PlayerItem track)
        {
            return track?.Track != null && track.Track.Source.StartsWith("http");
        }

        protected override bool CanAddToMyAudios(PlayerItem audio)
        {
            return audio?.Track?.VKInfo != null;
        }

        protected override bool CanAddSelectedAudios()
        {
            return SelectedItems.Cast<PlayerItem>().FirstOrDefault(CanAddToMyAudios) != null;
        }

        protected override bool CanDownloadSelected()
        {
            return SelectedItems.Cast<PlayerItem>().FirstOrDefault(CanDownloadTrack) != null;
        }

        protected override VKAudioInfo GetAudioInfo(PlayerItem track)
        {
            if (track.Track.VKInfo == null)
                return null;

            return new VKAudioInfo(track.Track.VKInfo.ID, track.Track.VKInfo.OwnerID);
        }

        protected override void CreateDefaultAppBarButtons()
        {
            base.CreateDefaultAppBarButtons();
            IsLockedPivot = false;
        }

        protected override void CreateSelectionAppBarButtons()
        {
            base.CreateSelectionAppBarButtons();
            IsLockedPivot = true;
        }

        private async void LoadPlayerState()
        {
            _appLoaderService.Show();
            RepeatMode = _playerService.RepeatMode;
            _isShuffleMode = _playerService.IsShuffleMode;
            OnPropertyChanged(nameof(IsShuffleMode));

            _isScrobbleMode = _playerService.IsScrobbleMode;
            OnPropertyChanged(nameof(IsScrobbleMode));
                
            IsPlaying = _playerService.CurrentState == PlayerState.Playing;
            _currentTrackId = _playerService.CurrentTrackID;

            Duration = _playerService.Duration;

            _position = _playerService.Position;
            OnPropertyChanged(nameof(Position));

            IEnumerable<IPlayerTrack> loadedPlaylist = null;
            if (IsShuffleMode)
                loadedPlaylist = await _playerPlaylistService.ReadShuffledPlaylist();
            else
                loadedPlaylist = await _playerPlaylistService.ReadPlaylist();

            if (loadedPlaylist == null)
            {
                await Task.Delay(1);
                _navigationService.Navigate("NoTracksView", null);
                _navigationService.RemoveLastPage("PlayerView");
                _appLoaderService.Hide();
                return;
            }
            else
                _tracks = loadedPlaylist.ToList();

            Tracks = new ObservableCollection<PlayerItem>(_tracks.Select(t => new PlayerItem { Track = t }));
                
            if (_currentTrackId >= 0)
            {
                CurrentTrack = Tracks[_currentTrackId];
                CurrentTrack.IsCurrent = true;

                LoadArtistImage(CurrentTrack.Track);
                LoadTrackImage(CurrentTrack.Track);
                ShowLyricsCommand.RaiseCanExecuteChanged();
                DownloadTrackCommand.RaiseCanExecuteChanged();
            }
            
            CanShuffle = true;
            _appLoaderService.Hide();
            UpdateAppBarState(CurrentPivotIndex);
        }

        private void Timer_Tick(object sender, object e)
        {
            _position = _playerService.Position;
            OnPropertyChanged(nameof(Position));

            Duration = _playerService.Duration;

            if (_noPositionUpdates)
                _noPositionUpdates = false;
        }

        private async void PlayerService_PlayerStateChanged(IPlayerService sender, PlayerStateChangedEventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (e.State == PlayerState.Playing)
                {
                    IsPlaying = true;
                    _timer.Start();
                }
                else
                {
                    IsPlaying = false;
                    _timer.Stop();
                }
            });
        }

        private async void PlayerService_TrackChanged(IPlayerService sender, TrackChangedEventArgs e)
        {
            _currentTrackId = e.TrackID;
            if (Tracks == null)
                return;

            if (_currentTrackId >= 0)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (CurrentTrack != null)
                        CurrentTrack.IsCurrent = false;

                    CurrentTrack = Tracks[_currentTrackId];
                    CurrentTrack.IsCurrent = true;

                    _position = TimeSpan.Zero;
                    OnPropertyChanged(nameof(Position));

                    Duration = TimeSpan.FromSeconds(1);

                    TrackChanged?.Invoke(this, CurrentTrack);
                    ShowLyricsCommand.RaiseCanExecuteChanged();
                    DownloadTrackCommand.RaiseCanExecuteChanged();
                });

                LoadArtistImage(CurrentTrack.Track);
                LoadTrackImage(CurrentTrack.Track);
            }
        }

        private async void LoadArtistImage(IPlayerTrack track)
        {
            string imagePath = await _imagesCacheService.GetCachedArtistImage(track.Artist);
            if (imagePath == null)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => ArtistImage = new BitmapImage(new Uri(AppConstants.DEFAULT_PLAYER_BACKGROUND_IMAGE)));
                _artistImageUrl = AppConstants.DEFAULT_PLAYER_BACKGROUND_IMAGE;

                imagePath = await _imagesCacheService.CacheAndGetArtistImage(track.Artist);
            }

            if (imagePath != null && ReferenceEquals(CurrentTrack.Track, track))
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => ArtistImage = new BitmapImage(new Uri(imagePath)));
                _artistImageUrl = imagePath;
            }
        }

        private async void LoadTrackImage(IPlayerTrack track)
        {
            string imagePath = await _imagesCacheService.GetCachedAlbumImage(track.Title);
            if (imagePath == null)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => TrackImage = null);

                imagePath = await _imagesCacheService.CacheAndGetAlbumImage(track.Title, track.Artist);
            }

            if (imagePath != null && ReferenceEquals(CurrentTrack.Track, track))
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => TrackImage = new BitmapImage(new Uri(imagePath)));
            }
        }

        private async void ShuffleTracks(bool isOn)
        {
            if (CurrentTrack == null)
            {
                _isShuffleMode = !_isShuffleMode;
                OnPropertyChanged(nameof(IsShuffleMode));
            }

            CanShuffle = false;
            var currentTrack = CurrentTrack.Track;

            if (isOn)
            {
                await _tracksShuffleSevice.ShuffleTracksAsync(_tracks, _currentTrackId);
                await _playerPlaylistService.WriteShuffledPlaylist(_tracks);
                Tracks = new ObservableCollection<PlayerItem>(_tracks.Select(t => new PlayerItem { Track = t }));
                _playerService.IsShuffleMode = true;

                _currentTrackId = _tracks.IndexOf(currentTrack);
            }
            else
            {
                _tracks = (await _playerPlaylistService.ReadPlaylist()).ToList();
                Tracks = new ObservableCollection<PlayerItem>(_tracks.Select(t => new PlayerItem { Track = t }));
                _playerService.IsShuffleMode = false;

                _currentTrackId = _tracks.IndexOf(currentTrack);
            }

            if (_currentTrackId >= 0)
            {
                CurrentTrack = Tracks[_currentTrackId];
                CurrentTrack.IsCurrent = true;
            }

            CanShuffle = true;
        }

        private void OnPlayTrackCommand(PlayerItem item)
        {
            int index = Tracks.IndexOf(item);
            if (index == -1)
                return;

            _playerService.PlayFromID(index);
        }

        private void OnNextTrackCommand()
        {
            _playerService.SkipNext();
        }

        private void OnPreviosTrackCommand()
        {
            _playerService.SkipPrevious();
        }

        private void OnPlayPauseCommand()
        {
            _playerService.PlayPause();
        }

        private void OnShowLyricsCommand()
        {
            if (CurrentTrack == null || _artistImageUrl == null)
                return;

            var track = CurrentTrack.Track;
            _navigationService.Navigate("TrackLyricsView",
                JsonConvert.SerializeObject(new KeyValuePair<string, string>(
                    JsonConvert.SerializeObject(track),
                    _artistImageUrl)));
        }

        private void UpdateAppBarState(int pivotIndex)
        {
            if (pivotIndex == 1)
                SetDefaultMode();
            else
            {
                AppBarItems.Clear();
                SecondaryItems.Clear();
            }
        }

        private void TryEnableScrobbleMode()
        {
            if (_purchaseService.IsFullVersionPurchased)
            {
                if (_lastFmLoginService.IsAuthorized)
                {
                    _isScrobbleMode = true;
                    _playerService.IsScrobbleMode = true;
                    OnPropertyChanged(nameof(IsScrobbleMode));
                }
                else
                {
                    _navigationService.Navigate("LastFmLoginView", null);
                }
            }
            else
            {
                if (_lastFmLoginService.IsAuthorized)
                {
                    _navigationService.Navigate("PurchaseView", null);
                }
                else
                {
                    _navigationService.Navigate("PurchaseView",
                        JsonConvert.SerializeObject(new KeyValuePair<string, string>("LastFmLoginView", null)));
                }
            }
        }

        private bool _isScrobbleMode;
        private bool _isSubscribed;
        private bool _isShuffleMode;        
        private PlayerRepeatMode _repeatMode;
        private int _currentTrackId;
        private TimeSpan _position;
        private List<IPlayerTrack> _tracks;
        private string _artistImageUrl;
        private int _currentPivotIndex;
        private bool _noPositionUpdates;    // Позволяет убрать заикание при переходе на страницу с работающим плеером.

        private readonly DispatcherTimer _timer;
        private readonly IPlayerPlaylistService _playerPlaylistService;
        private readonly IImagesCacheService _imagesCacheService;
        private readonly ITracksShuffleService _tracksShuffleSevice;
        private readonly ILastFmLoginService _lastFmLoginService;
        private readonly IPurchaseService _purchaseService;

        [ImplementPropertyChanged]
        public sealed class PlayerItem : IEquatable<PlayerItem>
        {
            public IPlayerTrack Track { get; set; }

            public bool IsCurrent { get; set; }

            public bool Equals(PlayerItem other)
            {
                return Track.Equals(other.Track);
            }
        }
    }
}
