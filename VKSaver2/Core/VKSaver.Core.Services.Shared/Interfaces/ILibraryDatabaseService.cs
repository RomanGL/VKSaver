using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Database;
using VKSaver.Core.Services.Database;

namespace VKSaver.Core.Services.Interfaces
{
    public interface ILibraryDatabaseService
    {
        event TypedEventHandler<ILibraryDatabaseService, DBUpdateProgressChangedEventArgs> UpdateProgressChanged;

        bool NeedReloadLibraryView { get; set; }

        void Update();

        Task<List<VKSaverTrack>> GetAllTracks();
        Task<List<VKSaverTrack>> GetAllCachedTracks();
        Task<List<VKSaverArtist>> GetAllArtists();
        Task<List<VKSaverAlbum>> GetAllAlbums();
        Task<List<VKSaverGenre>> GetAllGenres();
        Task<List<VKSaverFolder>> GetAllFolders();

        Task<VKSaverArtist> GetArtist(string dbKey);
        Task<VKSaverAlbum> GetAlbum(string dbKey);
        Task<VKSaverGenre> GetGenre(string dbKey);
        Task<VKSaverFolder> GetFolder(string dbKey);

        Task<List<T>> GetItems<T>(Func<T, bool> selector) where T : class;

        Task RemoveItem<T>(T item);
        Task RemoveItemByPrimaryKey<T>(object primaryKey);

        Task InsertDownloadedTrack(VKSaverAudio audio, string folderPath, string filePath);

        LibraryDatabaseCleaner GetCleaner();
    }
}
