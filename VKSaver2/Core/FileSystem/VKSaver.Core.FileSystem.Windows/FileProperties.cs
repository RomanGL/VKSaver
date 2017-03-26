using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VKSaver.Core.FileSystem
{
    public sealed class FileProperties : IFileProperties
    {
        public Task<MusicProperties> GetMusicPropertiesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<string, object>> RetrievePropertiesAsync(IEnumerable<string> propertiesToRetrieve)
        {
            throw new NotImplementedException();
        }
    }
}
