using System;

namespace VKSaver.Core.Services.Database
{
    public sealed class DBUpdateProgressChangedEventArgs : EventArgs
    {
        public DatabaseUpdateStepType Step { get; set; }

        public int CurrentItem { get; set; }
        public int TotalItems { get; set; }
    }
}
