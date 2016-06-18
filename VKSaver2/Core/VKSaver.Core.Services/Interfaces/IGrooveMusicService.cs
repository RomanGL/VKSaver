using System.Threading.Tasks;

namespace VKSaver.Core.Services.Interfaces
{
    /// <summary>
    /// Представляет сервис для работы с Groove Music.
    /// </summary>
    public interface IGrooveMusicService
    {
        /// <summary>
        /// Возвращает ссылку на изображение исполнителя.
        /// </summary>
        /// <param name="artistName">Имя исполнителя.</param>
        Task<string> GetArtistImageURL(string artistName);

        /// <summary>
        /// Возвращает ссылку на изображение трека.
        /// </summary>
        /// <param name="trackName">Название трека.</param>
        /// <param name="artistName">Имя исполнителя.</param>
        Task<string> GetTrackImageUrl(string trackName, string artistName);
    }
}