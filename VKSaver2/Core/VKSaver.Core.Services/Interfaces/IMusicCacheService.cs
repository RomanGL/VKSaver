using System.IO;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Common;
using Windows.Storage;
using Windows.Storage.Streams;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IMusicCacheService
    {
        Task<Stream> GetCachedFileStream(string fileName);

        Task<bool> ConvertAudioToVKSaverFormat(StorageFile file);
    }
}
