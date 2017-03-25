using System;
using System.IO;
using System.Threading.Tasks;
using VKSaver.Core.FileSystem;

namespace VKSaver.Core.Models.Common
{
    public interface IDataSource : IDisposable
    {
        IFile GetFile();
        Stream GetDataStream();
        Task<Stream> GetDataStreamAsync();
        string GetContentType();
    }
}
