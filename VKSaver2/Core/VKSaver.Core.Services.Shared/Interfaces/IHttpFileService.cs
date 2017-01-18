using System.Threading.Tasks;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IHttpFileService
    {
        Task<FileSize> GetFileSize(string url);
    }
}
