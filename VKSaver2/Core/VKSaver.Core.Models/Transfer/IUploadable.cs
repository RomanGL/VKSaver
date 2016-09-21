using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Models.Transfer
{
    public interface IUploadable
    {
        string Name { get; }
        string Extension { get; }
        FileContentType ContentType { get; }
        IDataSource Source { get; }
    }
}
