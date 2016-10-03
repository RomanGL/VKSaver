using System;
using VKSaver.Core.Models.Player;

namespace VKSaver.PlayerTask
{
    internal sealed class ManagerTrackChangedEventArgs : EventArgs
    {
        internal ManagerTrackChangedEventArgs(IPlayerTrack track, int id)
        {
            Track = track;
            TrackID = id;
        }

        public IPlayerTrack Track { get; private set; }

        public int TrackID { get; private set; }
    }
}
