using System.Collections.Generic;
using System.Threading.Tasks;

namespace VKSaver.Core.FileSystem
{
    public interface IFileProperties
    {
        Task<MusicProperties> GetMusicPropertiesAsync();

        Task<IDictionary<string, object>> RetrievePropertiesAsync(IEnumerable<string> propertiesToRetrieve);
    }
}
