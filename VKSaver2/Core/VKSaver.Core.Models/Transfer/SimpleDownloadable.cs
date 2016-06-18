using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Models.Transfer
{
    public class SimpleDownloadable : IDownloadable
    {
        public FileContentType ContentType { get; set; }

        public string Extension { get; set; }

        public string FileName { get; set; }

        public string Source { get; set; }
    }
}
