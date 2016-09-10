using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using Windows.Storage;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IMusicCacheService
    {
        Task<VKSaverAudioFile> GetVKSaverFile(string fileName);

        Task<bool> ConvertAudioToVKSaverFormat(StorageFile file, VKSaverAudio metadata);
    }
}
