using System;
using PropertyChanged;
using VKSaver.Core.Models.Player;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class PlayerItem : IEquatable<PlayerItem>
    {
        public IPlayerTrack Track { get; set; }

        public bool IsCurrent { get; set; }

        public bool Equals(PlayerItem other)
        {
            return Track.Equals(other.Track);
        }
    }
}
