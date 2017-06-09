using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Playback;
using Windows.UI.Notifications;
using NotificationsExtensions.TileContent;
using Prism.Events;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Services.Events;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Services.Player;
using static VKSaver.Core.Services.PlayerConstants;

namespace VKSaver.Core.Services
{
    public sealed class UwpPlayerService : IPlayerService
    {
        public event TypedEventHandler<IPlayerService, TrackChangedEventArgs> TrackChanged;
        public event TypedEventHandler<IPlayerService, PlayerStateChangedEventArgs> PlayerStateChanged;

        public UwpPlayerService(
            ILogService logService,
            IPlayerPlaylistService playerPlaylistService,
            ITracksShuffleService tracksShuffleService,
            ISettingsService settingsService,
            IEventAggregator eventAggregator,
            IMusicCacheService musicCacheService,
            IImagesCacheService imagesCacheService)
        {
            _logService = logService;
            _playerPlaylistService = playerPlaylistService;
            _tracksShuffleService = tracksShuffleService;
            _settingsService = settingsService;
            _eventAggregator = eventAggregator;
            _imagesCacheService = imagesCacheService;
            
            CurrentPlayer = new MediaPlayer();
            CurrentPlayer.CommandManager.IsEnabled = false;
            _manager = new PlaybackManager(CurrentPlayer, _settingsService, _playerPlaylistService, _logService, musicCacheService);

            _controls = CurrentPlayer.SystemMediaTransportControls;
            _controls.ButtonPressed += Controls_ButtonPressed;
        }

        public PlayerState CurrentState
        {
            get
            {
                if (CurrentPlayer.PlaybackSession == null)
                    return PlayerState.Closed;
                return (PlayerState)(int)CurrentPlayer.PlaybackSession.PlaybackState;
            }
        }

        public int CurrentTrackID => _manager.CurrentTrackID;

        public TimeSpan Duration
        {
            get
            {
                var time = CurrentPlayer.PlaybackSession?.NaturalDuration ?? TimeSpan.FromSeconds(1);
                if (time.TotalSeconds <= 0)
                    return TimeSpan.FromSeconds(1);
                return time;
            }
        }

        public TimeSpan Position
        {
            get
            {
                var time = CurrentPlayer.PlaybackSession?.Position ?? TimeSpan.Zero;
                if (time.TotalSeconds < 0)
                    return TimeSpan.Zero;
                return time;
            }
            set
            {
                if (CurrentPlayer.PlaybackSession != null)
                    CurrentPlayer.PlaybackSession.Position = value;
            }
        }

        public PlayerRepeatMode RepeatMode
        {
            get { return _settingsService.GetNoCache(PLAYER_REPEAT_MODE, PlayerRepeatMode.Disabled); }
            set
            {
                _settingsService.Set(PLAYER_REPEAT_MODE, value);
                _manager.UpdateRepeatMode();
            }
        }

        public bool IsShuffleMode
        {
            get { return _settingsService.GetNoCache(PLAYER_SHUFFLE_MODE, false); }
            set
            {
                _settingsService.Set(PLAYER_SHUFFLE_MODE, value);
                _manager.UpdateShuffleMode();
            }
        }

        public bool IsScrobbleMode
        {
            get { return _settingsService.GetNoCache(PLAYER_SCROBBLE_MODE, false); }
            set
            {
                _settingsService.Set(PLAYER_SCROBBLE_MODE, value);
                _manager.UpdateScrobbleMode();
            }
        }

        private MediaPlayer CurrentPlayer { get; }

        public void StartService()
        {
            SubscribeEvents();
        }

        public void StopService()
        {
            UnsubscribeEvents();
        }

        public void PlayFromID(int id)
        {
            _manager.PlayTrackFromID(id);
        }

        public void PlayPause()
        {
            _manager.PlayResume();
        }

        public void SkipNext()
        {
            _manager.NextTrack();
        }

        public void SkipPrevious()
        {
            _manager.PreviousTrack();
        }

        public async void UpdateLastFm()
        {
            await _manager.UpdateLastFm();
        }

        public async Task PlayNewTracks(IEnumerable<IPlayerTrack> tracks, int id)
        {
            await _playerPlaylistService.WritePlaylist(tracks);
            
            if (IsShuffleMode)
            {
                var shuffledTracks = new List<IPlayerTrack>(tracks);
                await _tracksShuffleService.ShuffleTracksAsync(shuffledTracks, id);
                await _playerPlaylistService.WriteShuffledPlaylist(shuffledTracks);

                _settingsService.Set(PLAYER_TRACK_ID, 0);
            }
            else
            {
                _settingsService.Set(PLAYER_TRACK_ID, id);
            }
            
            _eventAggregator.GetEvent<PlayNewTracksEvent>().Publish();
            await _manager.Update();
            PlayFromID(id);
        }

        private void SubscribeEvents()
        {
            lock (_lockObject)
            {
                if (_isSubscribed) return;

                UpdateLastFm();
                CurrentPlayer.CurrentStateChanged += Player_CurrentStateChanged;
                _manager.TrackChanged += Manager_TrackChanged;
                _isSubscribed = true;
            }
        }

        private void UnsubscribeEvents()
        {
            lock (_lockObject)
            {
                if (!_isSubscribed) return;

                CurrentPlayer.CurrentStateChanged -= Player_CurrentStateChanged;
                _manager.TrackChanged -= Manager_TrackChanged;
                _isSubscribed = false;
            }
        }

        private void Player_CurrentStateChanged(MediaPlayer sender, object args)
        {
            PlayerStateChanged?.Invoke(this,
                new PlayerStateChangedEventArgs((PlayerState)(int)sender.PlaybackSession.PlaybackState));

            switch (sender.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.None:
                    _controls.PlaybackStatus = MediaPlaybackStatus.Closed;
                    break;
                case MediaPlaybackState.Opening:
                    _controls.PlaybackStatus = MediaPlaybackStatus.Changing;
                    break;
                case MediaPlaybackState.Buffering:
                    _controls.PlaybackStatus = MediaPlaybackStatus.Changing;
                    break;
                case MediaPlaybackState.Playing:
                    _controls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    break;
                case MediaPlaybackState.Paused:
                    _controls.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;
            }
        }

        private void Manager_TrackChanged(object sender, ManagerTrackChangedEventArgs e)
        {
            TrackChanged?.Invoke(this, new TrackChangedEventArgs(e.TrackID));

            UpdateControlsOnNewTrack(e.Track);
            UpdateTileOnNewTrack(e.Track);

            _controls.IsEnabled = true;
            _controls.IsPlayEnabled = true;
            _controls.IsPauseEnabled = true;
            _controls.IsNextEnabled = true;
            _controls.IsPreviousEnabled = true;
        }

        private void Controls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            try
            {
                switch (args.Button)
                {
                    case SystemMediaTransportControlsButton.Play:
                        CurrentPlayer.Play();
                        break;
                    case SystemMediaTransportControlsButton.Pause:
                        CurrentPlayer.Pause();
                        break;
                    case SystemMediaTransportControlsButton.Next:
                        _manager.NextTrack();
                        break;
                    case SystemMediaTransportControlsButton.Previous:
                        _manager.PreviousTrack();
                        break;
                }
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex);
            }
        }

        private void UpdateControlsOnNewTrack(IPlayerTrack newTrack)
        {
            _controls.DisplayUpdater.Type = MediaPlaybackType.Music;
            _controls.DisplayUpdater.MusicProperties.Title = newTrack.Title;
            _controls.DisplayUpdater.MusicProperties.Artist = newTrack.Artist;
            _controls.DisplayUpdater.Update();
        }

        private async void UpdateTileOnNewTrack(IPlayerTrack track)
        {
            try
            {
                string artistImage = await _imagesCacheService.GetCachedArtistImage(track.Artist);

                var images = await _imagesCacheService.GetCachedAlbumsImages(3);
                if (images.Count == 1)
                    images.Add("ms-appx:///Assets/Images/PlayerLogo2.png");

                int count = 5 - images.Count;
                for (int i = 1; i <= count; i++)
                    images.Add($"ms-appx:///Assets/Images/PlayerLogo{i}.png");

                var wideTile = TileContentFactory.CreateTileWide310x150PeekImageCollection01();
                wideTile.TextBodyWrap.Text = track.Title;
                wideTile.TextHeading.Text = track.Artist;

                wideTile.ImageMain.Src = images[0];
                wideTile.ImageSmallColumn1Row1.Src = images[1];
                wideTile.ImageSmallColumn1Row2.Src = images[2];
                wideTile.ImageSmallColumn2Row1.Src = images[3];
                wideTile.ImageSmallColumn2Row2.Src = images[4];

                var squareTile = TileContentFactory.CreateTileSquare150x150PeekImageAndText02();
                squareTile.TextBodyWrap.Text = track.Title;
                squareTile.TextHeading.Text = track.Artist;
                squareTile.Image.Src = String.IsNullOrEmpty(artistImage) ? images[0] : artistImage;

                wideTile.RequireSquare150x150Content = true;
                wideTile.Square150x150Content = squareTile;

                var tileNotification = wideTile.CreateNotification();

                if (_manager.CurrentTrack.Equals(track))
                    TileUpdateManager.CreateTileUpdaterForApplication("App").Update(tileNotification);
            }
            catch (Exception ex)
            {
                _logService.LogText($"Tile update exception: {ex.ToString()}");
            }
        }

        private bool _isSubscribed;
        private PlaybackManager _manager;
        private SystemMediaTransportControls _controls;

        private readonly object _lockObject = new object();
        private readonly ILogService _logService;
        private readonly IPlayerPlaylistService _playerPlaylistService;
        private readonly ITracksShuffleService _tracksShuffleService;
        private readonly ISettingsService _settingsService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IImagesCacheService _imagesCacheService;
    }
}
