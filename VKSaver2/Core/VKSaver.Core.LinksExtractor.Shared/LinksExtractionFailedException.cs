using System;

namespace VKSaver.Core.LinksExtractor
{
    public sealed class LinksExtractionFailedException : Exception
    {
        internal LinksExtractionFailedException(string message, string videoUrl)
            : base(message)
        {
            VideoUrl = videoUrl;
        }

        public string VideoUrl { get; private set; }
    }
}
