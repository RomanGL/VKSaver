using System.Collections.Generic;
using System.Threading.Tasks;
using VKSaver.Core.Models.Transfer;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IDownloadsServiceHelper
    {
        Task<bool> StartDownloadingAsync(IList<IDownloadable> items);
        Task<bool> StartDownloadingAsync(IDownloadable item);
    }
}
