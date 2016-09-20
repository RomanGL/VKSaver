using System.Threading.Tasks;
using VKSaver.Core.Models.Transfer;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IUploadsPostprocessor
    {
        Task ProcessUploadAsync(IUpload upload);
    }
}
