using System.Collections.Generic;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using Windows.Storage;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IMusicCacheService
    {
        Task<VKSaverAudioFile> GetVKSaverFile(string fileName);

        Task<IEnumerable<VKSaverAudioFile>> GetCachedFiles(uint count, uint offset);

        Task<IEnumerable<VKSaverAudioFile>> GetCachedFiles();

        Task<bool> ConvertAudioToVKSaverFormat(StorageFile file, VKSaverAudio metadata);

        Task<bool> ClearMusicCache();
    }
}
