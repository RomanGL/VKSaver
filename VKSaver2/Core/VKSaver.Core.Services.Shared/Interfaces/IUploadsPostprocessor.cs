using System.Threading.Tasks;
using VKSaver.Core.Models.Transfer;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IUploadsPostprocessor
    {
        Task<UploadsPostprocessorResultType> ProcessUploadAsync(ICompletedUpload upload);
    }
}
