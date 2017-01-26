using System.Threading.Tasks;
using VKSaver.Core.Models;

namespace VKSaver.Core.Services
{
    public interface IVKAdService
    {
        Task<VKAdData> GetAdDataAsync(string adId);
        Task ReportAdAsync(bool isSuccess);
    }
}