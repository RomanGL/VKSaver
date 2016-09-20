using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Models.Transfer
{
    public interface IUploadable
    {
        FileContentType ContentType { get; }
        IDataSource Source { get; }
    }
}
