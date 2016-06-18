using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using VKSaver.Providers;
using VKSaver.Enums.App;
using VKSaver.Service;
using System.Threading.Tasks;
using VKSaver.Core.Player;
using Windows.Media;
using VKSaver.Helpers;
using System.Diagnostics;
using Windows.UI.Core;

namespace VKSaver.Controls
{
    public sealed class CustomFrame : Frame, IPlayerService
    {
        SystemMediaTransportControls _controls;
        private MediaElement _mediaElement;
        private List<IAudioTrack> _tracks;
        private List<IAudioTrack> _shuffledTracks;
        private List<IAudioTrack> _normalList;
        private bool _isInitialized;
        private bool _isShuffleMode;
        private bool _isRepeatMode;
        private IAudioTrack _currentTrack;

        public CustomFrame()
        {
            this.DefaultStyleKey = typeof(CustomFrame);
        }

        /// <summary>
        /// Вызывается при построении шаблона.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _mediaElement = GetTemplateChild("mediaElement") as MediaElement;

            if (_mediaElement == null) throw new ArgumentNullException("CustomFrame", "MediaElement is null.");
            Initialize();
        }

        public void Initialize()
        {
            if (_isInitialized || _mediaElement == null) return;

            _controls = SystemMediaTransportControls.GetForCurrentView();
            _controls.IsEnabled = true;
            _controls.IsPlayEnabled = true;
            _controls.IsPauseEnabled = true;
            _controls.IsNextEnabled = true;
            _controls.IsPreviousEnabled = true;

            _controls.ButtonPressed += Controls_ButtonPressed;
            _mediaElement.CurrentStateChanged += MediaElement_CurrentStateChanged;
            _mediaElement.MediaEnded += MediaElement_MediaEnded;
            _isShuffleMode = ServiceHelper.SettingsService.PlayerShuffleMode;
            _isRepeatMode = ServiceHelper.SettingsService.PlayerRepeatMode;
            CurrentTrackID = ServiceHelper.SettingsService.CurrentTrackID;
            _isInitialized = true;
        }        

        public void Deinitialize()
        {

        }

        /// <summary>
        /// Возвращает или задает текущую позицию мультимедиа.
        /// </summary>
        public TimeSpan CurrentPosition
        {
            get { return _mediaElement.Position; }
            set
            {
                if (_mediaElement.CurrentState != (MediaElementState.Closed | MediaElementState.Stopped))
                    _mediaElement.Position = value;
            }
        }

        /// <summary>
        /// Возвращает текущее состояние проигрывателя.
        /// </summary>
        public PlayerState CurrentState
        {
            get { return (PlayerState)((byte)_mediaElement.CurrentState); }
        }
        /// <summary>
        /// Идентификатор текущего трека.
        /// </summary>
        public int CurrentTrackID { get; private set; }
        /// <summary>
        /// Длительност текущего трека.
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                if (_mediaElement.CurrentState == (MediaElementState.Closed | MediaElementState.Stopped))
                    return TimeSpan.Zero;
                return _mediaElement.NaturalDuration.TimeSpan;
            }
        }

        /// <summary>
        /// Происходит при изменении состояния проигрывателя.
        /// </summary>
        public event EventHandler<PlayerState> PlayerStateChanged = delegate { };
        /// <summary>
        /// Происходит при изменении идентификатора текущего трека.
        /// </summary>
        public event EventHandler<int> TrackIDChanged = delegate { };      

        /// <summary>
        /// Возвращает список перемешанных треков.
        /// </summary>
        public async Task<IEnumerable<IAudioTrack>> GetPlaylist()
        {
            if (_normalList != null) return _normalList;

            var file = await FileHelper.GetLocalFileFromName(AppConstants.PlaylistFileName);
            if (file == null) return null;

            string json = await FileHelper.ReadTextFromFile(file);
            if (String.IsNullOrEmpty(json)) return null;
            _normalList = new List<IAudioTrack>(JsonSerializationHelper.DeserializeFromJson<List<AudioTrack>>(json));

            return _normalList;
        }

        /// <summary>
        /// Возвращает список перемешанных треков.
        /// </summary>
        public async Task<IEnumerable<IAudioTrack>> GetShuffledPlaylist()
        {
            if (_shuffledTracks != null) return _shuffledTracks;

            var file = await FileHelper.GetLocalFileFromName(AppConstants.ShuffledPlaylistFileName);
            if (file == null) return null;

            string json = await FileHelper.ReadTextFromFile(file);
            if (String.IsNullOrEmpty(json)) return null;
            _shuffledTracks = new List<IAudioTrack>(JsonSerializationHelper.DeserializeFromJson<List<AudioTrack>>(json));

            return _shuffledTracks;
        }        

        /// <summary>
        /// Следующий трек.
        /// </summary>
        public void NextTrack()
        {
            if (_tracks == null)
            {
#if DEBUG
                Debug.WriteLine("PlayerService has empty tracks list. Cant play next track.");
#endif
                return;
            }

            int id = 0;
            if (CurrentTrackID + 1 != _tracks.Count) id = CurrentTrackID + 1;
            PlayFromID(id);
        }

        /// <summary>
        /// Предыдущий трек.
        /// </summary>
        public void PreviousTrack()
        {
            if (_tracks == null)
            {
#if DEBUG
                Debug.WriteLine("PlayerService has empty tracks list. Cant play previous track.");
#endif
                return;
            }

            if (_mediaElement.Position.TotalSeconds >= AppConstants.PlayerReplayAgainTriggerSeconds)
            {
                _mediaElement.Position = TimeSpan.Zero;
                return;
            }

            int id;
            if (CurrentTrackID - 1 == -1) id = _tracks.Count - 1;
            else id = CurrentTrackID - 1;

            PlayFromID(id);
        }

        /// <summary>
        /// Воспроизвести трек по его идентификатору в плейлисте.
        /// </summary>
        /// <param name="trackId">Идентификатор трека в плейлисте.</param>
        public void PlayFromID(int trackId)
        {
            if (_tracks == null)
            {
#if DEBUG
                Debug.WriteLine(String.Format("PlayerService has empty tracks list. Cant play from id \"{0}\".", trackId));
#endif
                return;
            }

            _currentTrack = _tracks[trackId];
            CurrentTrackID = trackId;
            if (_currentTrack.Source == null) return;
            _mediaElement.Source = new Uri(_currentTrack.Source);

            TrackIDChanged(this, CurrentTrackID);
            UpdateControlsOnNewTrack(_currentTrack);
        }        

        /// <summary>
        /// Приостановить или возобновить воспроизведение.
        /// </summary>
        public void ResumePause()
        {
            if (_tracks == null)
            {
#if DEBUG
                Debug.WriteLine("PlayerService has empty tracks list. Cant play/pause.");
#endif
                return;
            }

            switch (_mediaElement.CurrentState)
            {
                case MediaElementState.Playing:
                    _mediaElement.Pause();
                    break;
                case MediaElementState.Paused:
                    _mediaElement.Play();
                    break;
                case MediaElementState.Stopped:
                    PlayFromID(CurrentTrackID);
                    break;
                case MediaElementState.Closed:
                    PlayFromID(CurrentTrackID);
                    break;
            }
        }

        /// <summary>
        /// Установить новый список треков.
        /// </summary>
        /// <param name="tracks">Список треков.</param>
        public async Task SetNewPlaylist(IEnumerable<IAudioTrack> tracks)
        {
            _normalList = new List<IAudioTrack>(tracks);
            var list = tracks.Select(t => new AudioTrack
            {
                Title = t.Title,
                Artist = t.Artist,
                Source = t.Source,
                LyricsID = t.LyricsID
            });
            bool result = await FileHelper.WriteTextToFile(
                await FileHelper.CreateLocalFile(AppConstants.PlaylistFileName),
                JsonSerializationHelper.SerializeToJson(list));
            UpdateNewPlaylist();
        }

        /// <summary>
        /// Установить новый перемешанный список треков.
        /// </summary>
        /// <param name="tracks">Список треков.</param>
        public async Task SetNewShuffledPlaylist(IEnumerable<IAudioTrack> tracks)
        {
            _shuffledTracks = new List<IAudioTrack>(tracks);
            var list = tracks.Select(t => new AudioTrack
            {
                Title = t.Title,
                Artist = t.Artist,
                Source = t.Source,
                LyricsID = t.LyricsID
            });
            bool result = await FileHelper.WriteTextToFile(
                await FileHelper.CreateLocalFile(AppConstants.ShuffledPlaylistFileName),
                JsonSerializationHelper.SerializeToJson(list));
        }

        /// <summary>
        /// Обновить на новый плейлист.
        /// </summary>
        public void UpdateNewPlaylist()
        {
            if (_isShuffleMode)
                _tracks = _shuffledTracks;
            else
                _tracks = _normalList;
        }

        /// <summary>
        /// Обновить режим повтора.
        /// </summary>
        public void UpdateRepeatMode()
        {
            _isRepeatMode = ServiceHelper.SettingsService.PlayerRepeatMode;
        }

        /// <summary>
        /// Обновить режим перемешивания.
        /// </summary>
        public void UpdateShuffleMode()
        {
            _isShuffleMode = ServiceHelper.SettingsService.PlayerShuffleMode;
            UpdateNewPlaylist();
            CurrentTrackID = _tracks.IndexOf(_currentTrack);
        }

        /// <summary>
        /// Вызывается при нажатии на системные кнопки управления проигрывателем.
        /// </summary>
        private async void Controls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            if (!_isInitialized) return;

            await _mediaElement.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (args.Button)
                {
                    case SystemMediaTransportControlsButton.Play:
                        ResumePause();
                        break;
                    case SystemMediaTransportControlsButton.Pause:
                        ResumePause();
                        break;
                    case SystemMediaTransportControlsButton.Next:
                        NextTrack();
                        break;
                    case SystemMediaTransportControlsButton.Previous:
                        PreviousTrack();
                        break;
                }
            });            
        }

        /// <summary>
        /// Вызывается при изменении состояния проигрывателя.
        /// </summary>
        private void MediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            switch (_mediaElement.CurrentState)
            {
                case MediaElementState.Playing:
                    _controls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    break;
                case MediaElementState.Paused:
                    _controls.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;
                case MediaElementState.Stopped:
                    _controls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    break;
                case MediaElementState.Closed:
                    _controls.PlaybackStatus = MediaPlaybackStatus.Closed;
                    break;
                case MediaElementState.Opening:
                    _controls.PlaybackStatus = MediaPlaybackStatus.Changing;
                    break;
                case MediaElementState.Buffering:
                    _controls.PlaybackStatus = MediaPlaybackStatus.Changing;
                    break;
            }
            PlayerStateChanged(this, (PlayerState)((byte)_mediaElement.CurrentState));
        }

        /// <summary>
        /// Вызывается при завершении воспроизвежения мультимедиа.
        /// </summary>
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (_isRepeatMode)
            {
                _mediaElement.Position = TimeSpan.Zero;
                return;
            }
            NextTrack();
        }

        /// <summary>
        /// Обновить системные кнопки управления проигрывателем при старте нового трека.
        /// </summary>
        /// <param name="newTrack">Новый трек.</param>
        private void UpdateControlsOnNewTrack(IAudioTrack newTrack)
        {
            _controls.DisplayUpdater.Type = MediaPlaybackType.Music;
            _controls.DisplayUpdater.MusicProperties.Title = newTrack.Title;
            _controls.DisplayUpdater.MusicProperties.Artist = newTrack.Artist;
            _controls.DisplayUpdater.Update();
        }
    }
}
