using System.Collections.Generic;
using System.Threading.Tasks;
using VKSaver.Core.Models.Database;
using VKSaver.Core.Services.Database;

namespace VKSaver.Core.Services.Interfaces
{
    public interface ILibraryDatabaseService
    {
        event TypedEventHandler<ILibraryDatabaseService, DBUpdateProgressChangedEventArgs> UpdateProgressChanged;

        void Update();

        Task<List<VKSaverTrack>> GetAllTracks();
        Task<List<VKSaverArtist>> GetAllArtists();

        Task<VKSaverArtist> GetArtist(string dbKey);
    }
}
