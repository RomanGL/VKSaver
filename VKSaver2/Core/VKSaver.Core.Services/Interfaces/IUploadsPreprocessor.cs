using System.Threading.Tasks;
using VKSaver.Core.Models.Transfer;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IUploadsPreprocessor
    {
        Task<IUpload> ProcessUploadableAsync(IUploadable uploadable);
    }
}
