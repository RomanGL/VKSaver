using System;
using System.Threading.Tasks;
using VKSaver.Core.Models.Transfer;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IUploadsPreprocessor
    {
        Task<Tuple<IUpload, UploadsPreprocessorResultType>> ProcessUploadableAsync(IUploadable uploadable);
    }
}
