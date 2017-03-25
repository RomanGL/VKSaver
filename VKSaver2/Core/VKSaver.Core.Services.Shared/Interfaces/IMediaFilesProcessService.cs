using System.Collections.Generic;
using System.Threading.Tasks;
using VKSaver.Core.FileSystem;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IMediaFilesProcessService
    {
        Task ProcessFiles(IEnumerable<IFile> filesToOpen);
    }
}
