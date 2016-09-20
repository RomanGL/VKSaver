using System.IO;
using System.Threading.Tasks;

namespace VKSaver.Core.Models.Common
{
    public interface IDataSource
    {
        Stream GetDataStream();
        Task<Stream> GetDataStreamAsync();
    }
}
