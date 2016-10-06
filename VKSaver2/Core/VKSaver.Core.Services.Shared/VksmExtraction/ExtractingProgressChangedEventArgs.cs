using System;

namespace VKSaver.Core.Services.VksmExtraction
{
    public sealed class ExtractingProgressChangedEventArgs : EventArgs
    {
        internal ExtractingProgressChangedEventArgs(long totalBytes, long completedBytes)
        {
            TotalBytes = totalBytes;
            CompletedBytes = completedBytes;
        }

        public long TotalBytes { get; private set; }
        public long CompletedBytes { get; private set; }
    }
}
