using System.Collections.Generic;
using System.Threading.Tasks;
using VKSaver.Core.FileSystem;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IMusicCacheService
    {
        Task<VKSaverAudioFile> GetVKSaverFile(string fileName);

        Task<IEnumerable<VKSaverAudioFile>> GetCachedFiles(uint count, uint offset);

        Task<IEnumerable<VKSaverAudioFile>> GetCachedFiles();

        Task<bool> ConvertAudioToVKSaverFormat(IFile file, VKSaverAudio metadata);
        Task PostprocessAudioAsync(IFile file, VKSaverAudio metadata);

        Task<bool> ClearMusicCache();
    }
}
