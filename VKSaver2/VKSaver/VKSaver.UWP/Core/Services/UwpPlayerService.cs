using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class UwpPlayerService : IPlayerService
    {
        public event TypedEventHandler<IPlayerService, TrackChangedEventArgs> TrackChanged;
        public event TypedEventHandler<IPlayerService, PlayerStateChangedEventArgs> PlayerStateChanged;
        
        public int CurrentTrackID { get; }
        public TimeSpan Duration { get; }
        public TimeSpan Position { get; set; }
        public PlayerState CurrentState { get; }
        public PlayerRepeatMode RepeatMode { get; set; }
        public bool IsShuffleMode { get; set; }
        public bool IsScrobbleMode { get; set; }

        public void StartService()
        {
            throw new NotImplementedException();
        }

        public void StopService()
        {
            throw new NotImplementedException();
        }

        public void PlayFromID(int id)
        {
            throw new NotImplementedException();
        }

        public void PlayPause()
        {
            throw new NotImplementedException();
        }

        public void SkipNext()
        {
            throw new NotImplementedException();
        }

        public void SkipPrevious()
        {
            throw new NotImplementedException();
        }

        public void UpdateLastFm()
        {
            throw new NotImplementedException();
        }

        public Task PlayNewTracks(IEnumerable<IPlayerTrack> tracks, int id)
        {
            throw new NotImplementedException();
        }
    }
}
