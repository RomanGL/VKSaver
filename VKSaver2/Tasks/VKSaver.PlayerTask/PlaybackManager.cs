using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Interfaces;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using static VKSaver.Core.Services.PlayerConstants;

namespace VKSaver.PlayerTask
{
    internal class PlaybackManager
    {       
        public event EventHandler<ManagerTrackChangedEventArgs> TrackChanged;
        
        public PlaybackManager(MediaPlayer player, SettingsService settingsService,
            IPlayerPlaylistService playerPlaylistService, ILogService logService,
            IMusicCacheService musicCacheService)
        {
            _settingsService = settingsService;
            _player = player;
            _playerPlaylistService = playerPlaylistService;
            _logService = logService;
            _musicCasheService = musicCacheService;

            _player.AutoPlay = false;
            _player.MediaOpened += Player_MediaOpened;
            _player.MediaEnded += Player_MediaEnded;
            _player.MediaFailed += Player_MediaFailed;

            _currentTrackID = settingsService.GetNoCache(PLAYER_TRACK_ID, -1);
            _isShuffleMode = settingsService.GetNoCache(PLAYER_SHUFFLE_MODE, false);
            _repeatMode = settingsService.GetNoCache(PLAYER_REPEAT_MODE, PlayerRepeatMode.Disabled);
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
                _logService.LogText("PlaybackManager has empty tracks list. Cant play next track.");
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
                _logService.LogText("PlaybackManager has empty tracks list. Cant play previous track.");
                return;
            }

            if (_player.Position.TotalSeconds >= PlayerConstants.PLAYER_REPLAY_AGAIN_DELAY_SECONDS)
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
            if (!await HasTracks())
            {
                _logService.LogText($"PlaybackManager has empty tracks list. Cant play from id \"{trackID}\".");
                return;
            }

            var track = _playlist[trackID];
            CurrentTrack = track;
            CurrentTrackID = trackID;

            Debug.WriteLine($"Next track is: {track.Title}");

            TrackChanged?.Invoke(this, new ManagerTrackChangedEventArgs(CurrentTrack, CurrentTrackID));
            
            if (track.VKInfo != null)
            {
                var cachedStream = await _musicCasheService.GetCachedFileStream(
                    $"{track.VKInfo.OwnerID} {track.VKInfo.ID}.vksm");

                if (cachedStream != null)
                {
                    Debug.WriteLine($"Cached track found: {track.Title}");
                    _player.SetStreamSource(cachedStream.AsRandomAccessStream());
                    return;
                }
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

        private readonly SettingsService _settingsService;
        private readonly MediaPlayer _player;
        private readonly IPlayerPlaylistService _playerPlaylistService;
        private readonly ILogService _logService;
        private readonly IMusicCacheService _musicCasheService;
    }
}
