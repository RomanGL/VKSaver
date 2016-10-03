using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace VKSaver.Core.Models.Common
{
    public interface IDataSource : IDisposable
    {
        StorageFile GetFile();
        Stream GetDataStream();
        Task<Stream> GetDataStreamAsync();
        string GetContentType();
    }
}
