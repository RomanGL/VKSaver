using System;

namespace VKSaver.Core.LinksExtractor
{
    public sealed class UnsupportedServiceException : Exception
    {
        internal UnsupportedServiceException(string message, string videoUrl)
            : base(message)
        {
            VideoUrl = videoUrl;
        }

        public string VideoUrl { get; private set; }
    }
}
