using System.Threading.Tasks;
using VKSaver.Core.Models.Transfer;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IUploadsServiceHelper
    {
        Task<bool> StartUploadingAsync(IUploadable item);
    }
}
