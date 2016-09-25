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
        Task<List<VKSaverTrack>> GetAllCachedTracks();
        Task<List<VKSaverArtist>> GetAllArtists();
        Task<List<VKSaverAlbum>> GetAllAlbums();
        Task<List<VKSaverGenre>> GetAllGenres();
        Task<List<VKSaverFolder>> GetAllFolders();

        Task<VKSaverArtist> GetArtist(string dbKey);
    }
}
