using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IMediaFilesProcessService
    {
        Task ProcessFiles(IEnumerable<IStorageItem> filesToOpen);
    }
}
