using System;

namespace VKSaver.Core.Services
{
    public sealed class TrackChangedEventArgs : EventArgs
    {
        public TrackChangedEventArgs(int id)
        {
            TrackID = id;
        }

        public int TrackID { get; private set; }
    }
}
