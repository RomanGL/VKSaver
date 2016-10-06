using System;

namespace VKSaver.Core.Services.VksmExtraction
{
    public sealed class SearchingProgressChangedEventArgs : EventArgs
    {
        internal SearchingProgressChangedEventArgs(int totalFiles, int currentFiles)
        {
            TotalFiles = totalFiles;
            CurrentFile = currentFiles;
        }

        public int TotalFiles { get; private set; }
        public int CurrentFile { get; private set; }
    }
}
