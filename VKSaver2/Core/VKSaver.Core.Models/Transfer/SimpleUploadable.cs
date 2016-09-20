using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Models.Transfer
{
    public class SimpleUploadable : IUploadable
    {
        public FileContentType ContentType { get; set; }

        public IDataSource Source { get; set; }
    }
}
