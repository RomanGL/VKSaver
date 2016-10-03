using System;
using System.Globalization;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using Xbox.Music;
using Xbox.Music.Model.Filters;

namespace VKSaver.Core.Services
{
    /// <summary>
    /// Представляет сервис для работы с Groove Music.
    /// </summary>
    public sealed class GrooveMusicService : IGrooveMusicService
    {
        private const string CLIENT_SECRET = "SMiic6+O8YKa13ak4Jpd0+ncQaCTakYZWc7mQtJolFg=";
        private const string CLIENT_NAME = "vksaver";

        private static readonly FilterType[] Artistfilter = new FilterType[] { FilterType.Artists };
        private readonly RegionInfo Region;
        private bool _isInitialized;
        private XboxMusicServiceClient _client;

        /// <summary>
        /// Инициализирует новый экземпляр класса.
        /// </summary>
        public GrooveMusicService(ICultureProvider cultureProvider)
        {
            _client = new XboxMusicServiceClient(CLIENT_NAME, CLIENT_SECRET);
            Region = cultureProvider.GetRegionInfo();
        }

        /// <summary>
        /// Получить ключ доступа.
        /// </summary>
        private async Task GetAccessToken()
        {
            await _client.InitializeAccessTokenAsync();
            _isInitialized = true;
        }

        /// <summary>
        /// Проверяет инициализацию клиента.
        /// </summary>
        private async Task<bool> CheckInitialized()
        {
            try
            {
                if (!_isInitialized) await _client.InitializeAccessTokenAsync();
                return true;
            }
            catch (Exception) { return false; }
        }

        /// <summary>
        /// Получить изображение исполнителя.
        /// </summary>
        /// <param name="artistName">Имя исполнителя.</param>
        public async Task<string> GetArtistImageURL(string artistName)
        {
            if (!await CheckInitialized())
                return null;
            string result = null;

            try
            {
                var searchResponse = await _client.SearchAsync(artistName, Region, 1, Artistfilter);
                if (searchResponse != null && searchResponse.Artists != null && searchResponse.Artists.Items.Count != 0)
                {
                    var artist = searchResponse.Artists.Items[0];
                    result = artist.ImageUrl;
                    //result = artist.GetCustomImageUri(artist.Id, "en-us", PictureResizeMode.Crop, width, height).ToString();
                }
            }
            catch (ArgumentNullException)
            {
                _isInitialized = false;
            }
            return result;
        }

        /// <summary>
        /// Возвращает ссылку на изображение трека.
        /// </summary>
        /// <param name="trackName">Название трека.</param>
        /// <param name="artistName">Имя исполнителя.</param>
        public async Task<string> GetTrackImageUrl(string trackName, string artistName)
        {            
            if (!await CheckInitialized()) return null;
            string result = null;

            return result;
        }
    }
}
