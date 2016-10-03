using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VKSaver.Core.Models.Player;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IPlayerService : ISuspendingService
    {
        event TypedEventHandler<IPlayerService, TrackChangedEventArgs> TrackChanged;
        event TypedEventHandler<IPlayerService, PlayerStateChangedEventArgs> PlayerStateChanged;

        int CurrentTrackID { get; }
        TimeSpan Duration { get; }
        TimeSpan Position { get; set; }
        PlayerState CurrentState { get; }
        PlayerRepeatMode RepeatMode { get; set; }
        bool IsShuffleMode { get; set; }
        bool IsScrobbleMode { get; set; }

        void PlayFromID(int id);        
        void PlayPause();
        void SkipNext();
        void SkipPrevious();
        void UpdateLastFm();
        
        Task PlayNewTracks(IEnumerable<IPlayerTrack> tracks, int id);
    }
}
