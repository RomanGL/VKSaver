using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Threading;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage;
using static VKSaver.Core.Services.PlayerConstants;

namespace VKSaver.PlayerTask
{
    public sealed class AudioTask : IBackgroundTask
    {        
        /// <summary>
        /// Запускает фоновую задачу.
        /// </summary>
        /// <param name="taskInstance">Экземпляр фоновой задачи.</param>
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            _settingsService = new SettingsService();
            _logService = new LogService();
            _playerPlaylistService = new PlayerPlaylistService(_logService);
            _musicCacheService = new MusicCacheService();

            _player = BackgroundMediaPlayer.Current;
            _manager = new PlaybackManager(_player, _settingsService, _playerPlaylistService, _logService, _musicCacheService);
            
            _controls = SystemMediaTransportControls.GetForCurrentView();
            _controls.ButtonPressed += Controls_ButtonPressed;
            _player.CurrentStateChanged += Player_CurrentStateChanged;
            _manager.TrackChanged += Manager_TrackChanged;

            _controls.IsEnabled = true;
            _controls.IsPlayEnabled = true;
            _controls.IsPauseEnabled = true;
            _controls.IsNextEnabled = true;
            _controls.IsPreviousEnabled = true;

            taskInstance.Canceled += TaskInstance_Canceled;
            taskInstance.Task.Completed += Task_Completed;
            BackgroundMediaPlayer.MessageReceivedFromForeground += MessageReceivedFromForeground;

            //var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/TestTrack.vksm"));
            //var zip = new ZipFile((await file.OpenReadAsync()).AsStream());
            //var content = zip.GetEntry("content.vks");
            //var fileStream = zip.GetInputStream(content);

            _isAppRunning = _settingsService.GetNoCache<bool>(APP_RUNNING);
            _settingsService.Set(TASK_RUNNING, true);

            var valueSet = new ValueSet();
            valueSet.Add(TASK_RUNNING, true);
            BackgroundMediaPlayer.SendMessageToForeground(valueSet);

            _isTaskRunning = true;
            _taskRunning.Set();

            GC.Collect(2, GCCollectionMode.Forced);
        }

        /// <summary>
        /// Вызывается при завершении задачи.
        /// </summary>
        private void Task_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            _deferral.Complete();
        }

        /// <summary>
        /// Вызывается при отмене фоновой задачи.
        /// </summary>
        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            try
            {
                _isTaskRunning = false;
                _isAppRunning = false;
                _taskRunning.Reset();
                _operationCompleted.Set();

                _controls.ButtonPressed -= Controls_ButtonPressed;
                _player.CurrentStateChanged -= Player_CurrentStateChanged;
                _manager.TrackChanged -= Manager_TrackChanged;
                BackgroundMediaPlayer.MessageReceivedFromForeground -= MessageReceivedFromForeground;

                _manager = null;
                _settingsService.Set(TASK_RUNNING, false);
                _settingsService = null;
                _playerPlaylistService = null;
                _logService = null;
                _musicCacheService = null;
                _player = null;

                BackgroundMediaPlayer.Shutdown();
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex);
            }

            _deferral.Complete();
        }

        /// <summary>
        /// Вызывается при изменении текущего трека в менеджере.
        /// </summary>
        private void Manager_TrackChanged(object sender, ManagerTrackChangedEventArgs e)
        {
            UpdateControlsOnNewTrack(e.Track);

            var valueSet = new ValueSet();
            valueSet.Add(PLAYER_TRACK_ID, e.TrackID);
            BackgroundMediaPlayer.SendMessageToForeground(valueSet);
        }

        /// <summary>
        /// Вызывается при изменении состояния проигрывателя.
        /// </summary>
        private void Player_CurrentStateChanged(MediaPlayer sender, object args)
        {
            switch (_player.CurrentState)
            {
                case MediaPlayerState.Closed:
                    _controls.PlaybackStatus = MediaPlaybackStatus.Closed;
                    break;
                case MediaPlayerState.Opening:
                    _controls.PlaybackStatus = MediaPlaybackStatus.Changing;
                    break;
                case MediaPlayerState.Buffering:
                    _controls.PlaybackStatus = MediaPlaybackStatus.Changing;
                    break;
                case MediaPlayerState.Playing:
                    _controls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    break;
                case MediaPlayerState.Paused:
                    _controls.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;
                case MediaPlayerState.Stopped:
                    _controls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    break;
            }
        }

        /// <summary>
        /// Вызывается при нажатии на системные кнопки проигрывателя.
        /// </summary>
        private void Controls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            try
            {
                if (!_isTaskRunning)
                {
                    var p = BackgroundMediaPlayer.Current;
                    if (!_taskRunning.WaitOne(PLAYER_TASK_STARTING_DELAY))
                        return;
                }
                switch (args.Button)
                {
                    case SystemMediaTransportControlsButton.Play:
                        BackgroundMediaPlayer.Current.Play();
                        break;
                    case SystemMediaTransportControlsButton.Pause:
                        BackgroundMediaPlayer.Current.Pause();
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

        /// <summary>
        /// Вызывается при получении сообщения от задачи переднего плана.
        /// </summary>
        private async void MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            foreach (string key in e.Data.Keys)
            {
                _operationCompleted.WaitOne();
                _operationCompleted.Reset();

                switch (key)
                {
                    case UPDATE_PLAYLIST:
                        await _manager.Update();
                        break;
                    case PLAYER_SHUFFLE_MODE:
                        await _manager.UpdateShuffleMode();
                        break;
                    case PLAYER_REPEAT_MODE:
                        _manager.UpdateRepeatMode();
                        break;
                    case PLAYER_TRACK_ID:
                        _manager.PlayTrackFromID((int)e.Data[key]);
                        break;
                    case PLAYER_PLAY_PAUSE:
                        _manager.PlayResume();
                        break;
                    case PLAYER_NEXT_TRACK:
                        _manager.NextTrack();
                        break;
                    case PLAYER_PREVIOUS_TRACK:
                        _manager.PreviousTrack();
                        break;
                    case APP_RUNNING:
                        _isAppRunning = (bool)e.Data[key];
                        break;
                }

                _operationCompleted.Set();
            }
        }

        /// <summary>
        /// Обновить системные кнопки управления проигрывателем при старте нового трека.
        /// </summary>
        /// <param name="newTrack">Новый трек.</param>
        private void UpdateControlsOnNewTrack(IPlayerTrack newTrack)
        {
            _controls.DisplayUpdater.Type = MediaPlaybackType.Music;
            _controls.DisplayUpdater.MusicProperties.Title = newTrack.Title;
            _controls.DisplayUpdater.MusicProperties.Artist = newTrack.Artist;
            _controls.DisplayUpdater.Update();
        }

        private BackgroundTaskDeferral _deferral;
        private SystemMediaTransportControls _controls;
        private SettingsService _settingsService;
        private MusicCacheService _musicCacheService;
        private MediaPlayer _player;
        private PlaybackManager _manager;
        private ILogService _logService;
        private IPlayerPlaylistService _playerPlaylistService;
        private bool _isAppRunning;
        private bool _isTaskRunning;

        private readonly ManualResetEvent _taskRunning = new ManualResetEvent(false);
        private readonly ManualResetEvent _operationCompleted = new ManualResetEvent(true);        
    }
}
