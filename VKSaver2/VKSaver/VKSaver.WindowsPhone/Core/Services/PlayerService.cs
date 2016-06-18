using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneTeam.SDK.Core;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Services.Interfaces;
using System.Threading;
using Windows.Media.Playback;
using OneTeam.SDK.Core.Services.Interfaces;
using static VKSaver.Core.Services.PlayerConstants;
using Windows.Foundation.Collections;

namespace VKSaver.Core.Services
{
    public sealed class PlayerService : IPlayerService
    {
        public event TypedEventHandler<IPlayerService, PlayerStateChangedEventArgs> PlayerStateChanged;
        public event TypedEventHandler<IPlayerService, TrackChangedEventArgs> TrackChanged;

        public PlayerService(ILogService logService, IPlayerPlaylistService playerPlaylistService,
            ITracksShuffleService tracksShuffleService, ISettingsService settingsService)
        {
            _logService = logService;
            _playerPlaylistService = playerPlaylistService;
            _tracksShuffleService = tracksShuffleService;
            _settingsService = settingsService;

            _taskStarted = new AutoResetEvent(false);
        }

        #region Свойства

        public PlayerState CurrentState
        {
            get
            {
                if (!IsTaskRunning)
                    return PlayerState.Closed;
                return (PlayerState)(int)CurrentPlayer.CurrentState;
            }
        }

        public int CurrentTrackID
        {
            get { return _settingsService.GetNoCache(PLAYER_TRACK_ID, -1); }
        }

        public TimeSpan Duration
        {
            get
            {
                if (!IsTaskRunning)
                    return TimeSpan.FromSeconds(1);

                var time = CurrentPlayer.NaturalDuration;
                if (time.TotalSeconds <= 0)
                    return TimeSpan.FromSeconds(1);
                return time;
            }
        }

        public TimeSpan Position
        {
            get
            {
                if (!IsTaskRunning)
                    return TimeSpan.Zero;

                var time = CurrentPlayer.Position;
                if (time.TotalSeconds < 0)
                    return TimeSpan.Zero;
                return time;
            }
            set
            {
                if (IsTaskRunning)
                    CurrentPlayer.Position = value;
            }
        }

        public PlayerRepeatMode RepeatMode
        {
            get { return _settingsService.GetNoCache(PLAYER_REPEAT_MODE, PlayerRepeatMode.Disabled); }
            set
            {
                _settingsService.Set(PLAYER_REPEAT_MODE, value);
                SendMessageToBackground(PLAYER_REPEAT_MODE, null);
            }
        }

        public bool IsShuffleMode
        {
            get { return _settingsService.GetNoCache(PLAYER_SHUFFLE_MODE, false); }
            set
            {
                _settingsService.Set(PLAYER_SHUFFLE_MODE, value);
                SendMessageToBackground(PLAYER_SHUFFLE_MODE, null);
            }
        }

        private bool IsTaskRunning
        {
            get
            {
                return _settingsService.GetNoCache(TASK_RUNNING, false);
                //if (_isTaskRunning == null)
                //    _isTaskRunning = _settingsService.GetNoCache(TASK_RUNNING, false);
                //return _isTaskRunning.Value;
            }
        }

        private MediaPlayer CurrentPlayer
        {
            get
            {
                MediaPlayer player = null;
                int retryCount = 2;

                while (player == null || --retryCount >= 0)
                {
                    try
                    {
                        player = BackgroundMediaPlayer.Current;
                    }
                    catch (Exception ex)
                    {
                        _logService.LogException(ex);

                        if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                        {
                            ResetAfterLostBackground();
                            StartBackgroundAudioTask();
                        }                        
                    }
                }

                return player;
            }
        }

        #endregion

        public void StartService()
        {
            SubscribeEvents();
            _settingsService.Set(APP_RUNNING, true);
            //_isTaskRunning = null;            
        }

        public void StopService()
        {
            UnsubscribeEvents();
            _settingsService.Set(APP_RUNNING, false);
            //_isTaskRunning = null;            
        }

        public void PlayFromID(int id)
        {
            if (!IsTaskRunning)
                StartBackgroundAudioTask();

            _settingsService.Set(PLAYER_TRACK_ID, id);
            SendMessageToBackground(PLAYER_TRACK_ID, id);
        }

        public async Task PlayNewTracks(IEnumerable<IPlayerTrack> tracks, int id)
        {
            if (!IsTaskRunning)
                StartBackgroundAudioTask();

            await _playerPlaylistService.WritePlaylist(tracks);

            var message = new ValueSet();
            message.Add(UPDATE_PLAYLIST, null);

            if (IsShuffleMode)
            {
                var shuffledTracks = new List<IPlayerTrack>(tracks);
                await _tracksShuffleService.ShuffleTracksAsync(shuffledTracks, id);
                await _playerPlaylistService.WriteShuffledPlaylist(shuffledTracks);

                _settingsService.Set(PLAYER_TRACK_ID, 0);
                message.Add(PLAYER_TRACK_ID, 0);
            }
            else
            {
                _settingsService.Set(PLAYER_TRACK_ID, id);
                message.Add(PLAYER_TRACK_ID, id);
            }

            SendMessageToBackground(message);
        }

        public void PlayPause()
        {
            if (!IsTaskRunning)
                StartBackgroundAudioTask();

            SendMessageToBackground(PLAYER_PLAY_PAUSE, null);
        }

        public void SkipNext()
        {
            if (!IsTaskRunning)
                StartBackgroundAudioTask();

            SendMessageToBackground(PLAYER_NEXT_TRACK, null);
        }

        public void SkipPrevious()
        {
            if (!IsTaskRunning)
                StartBackgroundAudioTask();

            SendMessageToBackground(PLAYER_PREVIOUS_TRACK, null);
        }
        
        private void SubscribeEvents()
        {
            if (_isSubscribed) return;

            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground += MessageReceivedFromBackground;
                CurrentPlayer.CurrentStateChanged += Player_CurrentStateChanged;
                _isSubscribed = true;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                    ResetAfterLostBackground();
                else
                    new Exception("Не удалось запустить проигрыватель.", ex);
            }
        }

        private void UnsubscribeEvents()
        {
            if (!_isSubscribed) return;

            BackgroundMediaPlayer.MessageReceivedFromBackground -= MessageReceivedFromBackground;
            CurrentPlayer.CurrentStateChanged -= Player_CurrentStateChanged;
            _isSubscribed = false;
        }

        private void StartBackgroundAudioTask()
        {
            SubscribeEvents();

            if (!_taskStarted.WaitOne(PLAYER_TASK_STARTING_DELAY))
                throw new Exception("Проигрыватель не запустился за отведенное время.");
        }
                
        private void ResetAfterLostBackground()
        {
            BackgroundMediaPlayer.Shutdown();
            _settingsService.Set(TASK_RUNNING, false);
            _taskStarted.Reset();

            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground += MessageReceivedFromBackground;
                CurrentPlayer.CurrentStateChanged += Player_CurrentStateChanged;
                _isSubscribed = true;
            }
            catch (Exception ex)
            {
                throw new Exception("Не удалось запустить проигрыватель.", ex);
            }
        }

        private void MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            foreach (var key in e.Data.Keys)
            {
                switch (key)
                {
                    case TASK_RUNNING:
                        _taskStarted.Set();
                        _isTaskRunning = true;
                        break;
                    case PLAYER_TRACK_ID:
                        TrackChanged?.Invoke(this, new TrackChangedEventArgs((int)e.Data[key]));
                        break;
                }
            }
        }
                
        private void Player_CurrentStateChanged(MediaPlayer sender, object args)
        {
            PlayerStateChanged?.Invoke(this, 
                new PlayerStateChangedEventArgs((PlayerState)(int)sender.CurrentState));
        }

        private static void SendMessageToBackground(ValueSet message)
        {
            BackgroundMediaPlayer.SendMessageToBackground(message);
        }

        private static void SendMessageToBackground(string name, object parameter)
        {
            var valueSet = new ValueSet();
            valueSet.Add(name, parameter);
            BackgroundMediaPlayer.SendMessageToBackground(valueSet);
        }

        private bool _isSubscribed;
        private bool? _isTaskRunning;

        private readonly ILogService _logService;
        private readonly IPlayerPlaylistService _playerPlaylistService;
        private readonly ITracksShuffleService _tracksShuffleService;
        private readonly ISettingsService _settingsService;
        private readonly AutoResetEvent _taskStarted;

        private const int RPC_S_SERVER_UNAVAILABLE = -2147023174; // 0x800706BA
    }
}