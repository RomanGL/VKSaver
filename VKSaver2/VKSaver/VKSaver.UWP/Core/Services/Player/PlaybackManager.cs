#if !WINDOWS_UWP
using IF.Lastfm.SQLite;
#endif

using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using IF.Lastfm.Core.Scrobblers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Database;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Database;
using VKSaver.Core.Services.Interfaces;
using Windows.Media.Playback;
using Windows.Storage;
using static VKSaver.Core.Services.PlayerConstants;

namespace VKSaver.Core.Services.Player
{
    internal class PlaybackManager
    {       
        public event EventHandler<ManagerTrackChangedEventArgs> TrackChanged;
        
        public PlaybackManager(
            MediaPlayer player, 
            ISettingsService settingsService,
            IPlayerPlaylistService playerPlaylistService, 
            ILogService logService,
            IMusicCacheService musicCacheService)
        {
            _settingsService = settingsService;
            _player = player;
            _playerPlaylistService = playerPlaylistService;
            _logService = logService;
            _musicCasheService = musicCacheService;

            _database = new LibraryDatabase();

            _player.AutoPlay = false;
            _player.MediaOpened += Player_MediaOpened;
            _player.MediaEnded += Player_MediaEnded;
            _player.MediaFailed += Player_MediaFailed;

            _currentTrackID = settingsService.GetNoCache(PLAYER_TRACK_ID, -1);
            _isShuffleMode = settingsService.GetNoCache(PLAYER_SHUFFLE_MODE, false);
            UpdateRepeatMode();
            UpdateScrobbleMode();
        }

        #region Свойства

        public IPlayerTrack CurrentTrack { get; private set; }
        
        public int CurrentTrackID
        {
            get { return _currentTrackID; }
            private set
            {
                _currentTrackID = value;
                _settingsService.Set(PLAYER_TRACK_ID, value);
            }
        }

        #endregion

        #region Публичные методы

        public async Task Update()
        {
            await _database.Initialize();

            IEnumerable<IPlayerTrack> tracks = null;
            if (_isShuffleMode)
                tracks = await _playerPlaylistService.ReadShuffledPlaylist();
            else
                tracks = await _playerPlaylistService.ReadPlaylist();

            if (tracks != null)
                _playlist = new List<IPlayerTrack>(tracks);
        }
        
        public async Task UpdateShuffleMode()
        {
            _isShuffleMode = _settingsService.GetNoCache(PLAYER_SHUFFLE_MODE, false);
            await Update();
            CurrentTrackID = _playlist.IndexOf(CurrentTrack);
        }
        
        public void UpdateRepeatMode()
        {
            _repeatMode = _settingsService.GetNoCache(PLAYER_REPEAT_MODE, PlayerRepeatMode.Disabled);
        }

        public void UpdateScrobbleMode()
        {
            _isScrobbleMode = _settingsService.GetNoCache(PLAYER_SCROBBLE_MODE, false);
        }

        public async Task UpdateLastFm()
        {
            try
            {
                _lastAuth = new LastAuth(LAST_FM_API_KEY, LAST_FM_API_SECRET);
                var session = _settingsService.GetNoCache<LastUserSession>(LAST_FM_USER_SESSION_PARAMETER);

                if (session == null)
                {
                    _lastAuth = null;
                    _scrobbler = null;
                    return;
                }

                _lastAuth.LoadSession(session);

#if WINDOWS_UWP
                _scrobbler = new Scrobbler(_lastAuth);
#else
                var dbFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    "scrobbler.db", CreationCollisionOption.OpenIfExists);
                _scrobbler = new SQLiteScrobbler(_lastAuth, dbFile.Path);
#endif
            }
            catch (Exception ex)
            {
                _lastAuth = null;
                _scrobbler = null;

                _logService.LogException(ex);
            }
        }
        
        public async void PlayResume()
        {
            if (!await HasTracks()) return;

            switch (_player.CurrentState)
            {
                case MediaPlayerState.Playing:
                    _player.Pause();
                    break;
                case MediaPlayerState.Paused:
                    _player.Play();
                    break;
                case MediaPlayerState.Closed:
                    PlayTrackFromID(CurrentTrackID);
                    break;
                case MediaPlayerState.Stopped:
                    PlayTrackFromID(CurrentTrackID);
                    break;
            }
        }
        
        public async void NextTrack()
        {
            if (!await HasTracks())
            {
                return;
            }

            int id = 0;
            if (CurrentTrackID + 1 != _playlist.Count)
                id = CurrentTrackID + 1;
            PlayTrackFromID(id);
        }
        
        public async void PreviousTrack()
        {
            if (!await HasTracks())
            {
                return;
            }

            if (_player.Position.TotalSeconds >= PLAYER_REPLAY_AGAIN_DELAY_SECONDS)
            {
                _player.Position = TimeSpan.Zero;
                return;
            }

            int id;
            if (CurrentTrackID - 1 == -1)
                id = _playlist.Count - 1;
            else
                id = CurrentTrackID - 1;

            PlayTrackFromID(id);
        }
                
        public async void PlayTrackFromID(int trackID)
        {
            GC.Collect(1, GCCollectionMode.Forced);

            if (!await HasTracks())
            {
                return;
            }

            ScrobbleTrack(CurrentTrack);

            var track = _playlist[trackID];
            CurrentTrack = track;
            CurrentTrackID = trackID;

            Debug.WriteLine($"Next track is: {track.Title}");
            TrackChanged?.Invoke(this, new ManagerTrackChangedEventArgs(CurrentTrack, CurrentTrackID));

            StorageFile file = await MusicFilesPathHelper.GetFileFromCapatibleName(track.Source);
            bool skipMss = false;

            if (file == null)
            {
                string vkInfoKey = $"{track.VKInfo.OwnerID} {track.VKInfo.ID}";
                var dbTrack = await _database.FindItem<VKSaverTrack>(t => t.VKInfoKey == vkInfoKey);

                if (dbTrack != null)
                {
                    file = await MusicFilesPathHelper.GetFileFromCapatibleName(dbTrack.Source);
                    if (file != null)
                        skipMss = true;
                }
            }

            if (!skipMss && (track.VKInfo != null || (file != null && file.Path.EndsWith(".vksm"))))
            {
                var worker = new MediaStreamSourceWorker(CurrentTrack, _musicCasheService, file);
                var cachedSource = await worker.GetSource(); 

                if (cachedSource != null)
                {
                    Debug.WriteLine($"Cached track found: {track.Title}");
                    _player.SetMediaSource(cachedSource);
                    return;
                }
                else
                    worker.Dispose();
            }
            else if (file != null)
            {
                _player.SetFileSource(file);
                return;
            }

            if (String.IsNullOrEmpty(track.Source))
            {
                NextTrack();
                return;
            }

            _player.SetUriSource(new Uri(track.Source));  
        }

#endregion

        #region Приватные методы  
               
        private async void ScrobbleTrack(IPlayerTrack track)
        {
            try
            {
                if (!_isScrobbleMode || track == null || _scrobbler == null)
                    return;
                else if (_player.Position.TotalSeconds < _player.NaturalDuration.TotalSeconds / 1.5)
                    return;

                var response = await _scrobbler.ScrobbleAsync(
                    new Scrobble(track.Artist, null, track.Title, DateTimeOffset.Now));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
            }
        }

        private async Task<bool> HasTracks()
        {
            if (_playlist == null || _playlist.Count == 0)
            {
                await Update();
                return _playlist != null && _playlist.Count != 0;
            }
            return true;
        }
        
        private void Player_MediaOpened(MediaPlayer sender, object args)
        {
            _player.Play();
            Debug.WriteLine($"Track opened: {CurrentTrack.Title}");
        }
        
        private void Player_MediaEnded(MediaPlayer sender, object args)
        {
            Debug.WriteLine($"Track ended: {CurrentTrack.Title}");

            switch (_repeatMode)
            {
                case PlayerRepeatMode.One:
                    _player.Play();
                    break;
                case PlayerRepeatMode.All:
                    if (CurrentTrackID + 1 == _playlist.Count)
                        PlayTrackFromID(0);
                    else
                        PlayTrackFromID(CurrentTrackID + 1);
                    break;
                default:
                    if (CurrentTrackID + 1 != _playlist.Count)
                        PlayTrackFromID(CurrentTrackID + 1);
                    break;
            }
        }

        private void Player_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            Debug.WriteLine($"Track failed: {CurrentTrack.Title}\nInfo: {args.ExtendedErrorCode.Message}");
        }

        #endregion

        private List<IPlayerTrack> _playlist;
        private bool _isShuffleMode;
        private int _currentTrackID;
        private PlayerRepeatMode _repeatMode;
        private bool _isScrobbleMode;

        private IScrobbler _scrobbler;
        private ILastAuth _lastAuth;
        private LibraryDatabase _database;

        private readonly ISettingsService _settingsService;
        private readonly MediaPlayer _player;
        private readonly IPlayerPlaylistService _playerPlaylistService;
        private readonly ILogService _logService;
        private readonly IMusicCacheService _musicCasheService;        

        private const string LAST_FM_API_KEY = "***REMOVED***";
        private const string LAST_FM_API_SECRET = "***REMOVED***";
        private const string LAST_FM_USER_SESSION_PARAMETER = "LfUserSession";
    }
}
