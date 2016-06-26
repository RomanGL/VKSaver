using System.Collections.Generic;
using System.Threading.Tasks;
using VKSaver.Core.Models;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Services.Interfaces
{
    /// <summary>
    /// Представляет сервис кэширования изображений.
    /// </summary>
    public interface IImagesCacheService
    {
        Task<string> CacheAndGetArtistImage(string artistsName);

        Task<string> GetCachedArtistImage(string artistName);

        Task<string> CacheAndGetAlbumImage(string trackTitle, string artistName);

        Task<string> GetCachedAlbumImage(string trackTitle);

        Task<IList<string>> GetCachedAlbumsImages(uint count);

        /// <summary>
        /// Очистить кэш изображений альбомов треков.
        /// </summary>
        Task<bool> ClearAlbumsCache();

        /// <summary>
        /// Очистить кэш изображений исполнителей.
        /// </summary>
        Task<bool> ClearArtistsCache();

        /// <summary>
        /// Возвращает размер кэша изображений альбомов треков.
        /// </summary>
        Task<FileSize> GetAlbumsCacheSize();

        /// <summary>
        /// Возвращает размер кэша изображений исполнителей.
        /// </summary>
        Task<FileSize> GetArtistsCacheSize();
    }
}
