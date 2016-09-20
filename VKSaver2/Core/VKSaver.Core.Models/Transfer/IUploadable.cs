using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Models.Transfer
{
    public interface IUploadable
    {
        string Name { get; }
        FileContentType ContentType { get; }
        IDataSource Source { get; }
    }
}
