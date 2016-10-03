using System;
using VKSaver.Core.Models.Player;

namespace VKSaver.Core.Services
{
    public sealed class PlayerStateChangedEventArgs : EventArgs
    {
        public PlayerStateChangedEventArgs(PlayerState state)
        {
            State = state;
        }

        public PlayerState State { get; private set; }
    }
}
