using System;
using System.IO;
using System.Threading.Tasks;

namespace VKSaver.Core.Models.Common
{
    public interface IDataSource : IDisposable
    {
        Stream GetDataStream();
        Task<Stream> GetDataStreamAsync();
        string GetContentType();
    }
}
