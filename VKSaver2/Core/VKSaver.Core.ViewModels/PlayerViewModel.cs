using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Newtonsoft.Json;
using OneTeam.SDK.Core;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class PlayerViewModel : ViewModelBase
    {
        public event TypedEventHandler<PlayerViewModel, PlayerItem> TrackChanged;

        public PlayerViewModel(INavigationService navigationService, IPlayerService playerService,
            IPlayerPlaylistService playerPlaylistService, IImagesCacheService imagesCacheService,
            ITracksShuffleService tracksShuffleService, IDownloadsService downloadsService)
        {
            _navigationService = navigationService;
            _playerService = playerService;
            _playerPlaylistService = playerPlaylistService;
            _imagesCacheService = imagesCacheService;
            _tracksShuffleSevice = tracksShuffleService;
            _downloadsService = downloadsService;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };

            NextTrackCommand = new DelegateCommand(OnNextTrackCommand);
            PreviousTrackCommand = new DelegateCommand(OnPreviosTrackCommand);
            PlayPauseCommand = new DelegateCommand(OnPlayPauseCommand);
            PlayTrackCommand = new DelegateCommand<PlayerItem>(OnPlayTrackCommand);
            ShowLyricsCommand = new DelegateCommand(OnShowLyricsCommand,
                () => CurrentTrack != null && CurrentTrack.Track.LyricsID != 0);
            DownloadTrackCommand = new DelegateCommand<PlayerItem>(OnDownloadTrackCommand);
        }

        public PlayerItem CurrentTrack { get; private set; }

        public ImageSource ArtistImage { get; private set; }

        public ImageSource TrackImage { get; private set; }

        public ObservableCollection<PlayerItem> Tracks { get; private set; }

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

        [DoNotNotify]
        public DelegateCommand<PlayerItem> DownloadTrackCommand { get; private set; }

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
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async void LoadPlayerState()
        {
            RepeatMode = _playerService.RepeatMode;
            _isShuffleMode = _playerService.IsShuffleMode;
            OnPropertyChanged(nameof(IsShuffleMode));
                
            IsPlaying = _playerService.CurrentState == PlayerState.Playing;
            _currentTrackID = _playerService.CurrentTrackID;

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
                return;
            }
            else
                _tracks = loadedPlaylist.ToList();

            Tracks = new ObservableCollection<PlayerItem>(_tracks.Select(t => new PlayerItem { Track = t }));
                
            if (_currentTrackID >= 0)
            {
                CurrentTrack = Tracks[_currentTrackID];
                CurrentTrack.IsCurrent = true;

                LoadArtistImage(CurrentTrack.Track);
                LoadTrackImage(CurrentTrack.Track);
                ShowLyricsCommand.RaiseCanExecuteChanged();
            }

            CanShuffle = true;
        }

        private void Timer_Tick(object sender, object e)
        {
            _position = _playerService.Position;
            OnPropertyChanged(nameof(Position));

            Duration = _playerService.Duration;
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
                    _timer.Stop();
            });
        }

        private async void PlayerService_TrackChanged(IPlayerService sender, TrackChangedEventArgs e)
        {
            _currentTrackID = e.TrackID;
            if (Tracks == null)
                return;

            if (_currentTrackID >= 0)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (CurrentTrack != null)
                        CurrentTrack.IsCurrent = false;

                    CurrentTrack = Tracks[_currentTrackID];
                    CurrentTrack.IsCurrent = true;

                    _position = TimeSpan.Zero;
                    OnPropertyChanged(nameof(Position));

                    Duration = TimeSpan.FromSeconds(1);

                    TrackChanged?.Invoke(this, CurrentTrack);
                    ShowLyricsCommand.RaiseCanExecuteChanged();
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
                    () => ArtistImage = new BitmapImage(new Uri(DEFAULT_BACKGROUND_IMAGE)));
                _artistImageUrl = DEFAULT_BACKGROUND_IMAGE;

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
                await _tracksShuffleSevice.ShuffleTracksAsync(_tracks, _currentTrackID);
                await _playerPlaylistService.WriteShuffledPlaylist(_tracks);
                Tracks = new ObservableCollection<PlayerItem>(_tracks.Select(t => new PlayerItem { Track = t }));
                _playerService.IsShuffleMode = true;

                _currentTrackID = _tracks.IndexOf(currentTrack);
            }
            else
            {
                _tracks = (await _playerPlaylistService.ReadPlaylist()).ToList();
                Tracks = new ObservableCollection<PlayerItem>(_tracks.Select(t => new PlayerItem { Track = t }));
                _playerService.IsShuffleMode = false;

                _currentTrackID = _tracks.IndexOf(currentTrack);
            }

            if (_currentTrackID >= 0)
            {
                CurrentTrack = Tracks[_currentTrackID];
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

        private async void OnDownloadTrackCommand(PlayerItem item)
        {
            var res = await _downloadsService.StartDownloadingAsync(
                new List<IDownloadable> { item.Track as IDownloadable });
        }

        private bool _isSubscribed;
        private bool _isShuffleMode;        
        private PlayerRepeatMode _repeatMode;
        private int _currentTrackID;
        private TimeSpan _position;
        private List<IPlayerTrack> _tracks;
        private string _artistImageUrl;
        private bool _noPositionUpdates;    // Позволяет убрать заикание при переходе на страницу с работающим плеером.

        private readonly DispatcherTimer _timer;
        private readonly INavigationService _navigationService;
        private readonly IPlayerService _playerService;
        private readonly IPlayerPlaylistService _playerPlaylistService;
        private readonly IImagesCacheService _imagesCacheService;
        private readonly ITracksShuffleService _tracksShuffleSevice;
        private readonly IDownloadsService _downloadsService;

        private const string DEFAULT_BACKGROUND_IMAGE = "ms-appx:///Assets/Background/PlayerBackground.png";

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
