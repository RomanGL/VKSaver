using System;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Models.Transfer
{
    public interface ICompletedUpload
    {
        Guid Id { get; }
        string Name { get; }
        FileContentType ContentType { get; }
        string ServerResponse { get; }
    }
}
