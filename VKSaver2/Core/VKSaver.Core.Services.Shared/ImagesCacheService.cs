using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Interfaces;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Web.Http;
using IF.Lastfm.Core.Api;
using ModernDev.InTouch;

namespace VKSaver.Core.Services
{
    /// <summary>
    /// Представляет сервис кэширования изображений.
    /// </summary>
    public sealed class ImagesCacheService : IImagesCacheService
    {    
        public ImagesCacheService(
            IGrooveMusicService grooveMusicService, 
            LastfmClient lfClient,
            INetworkInfoService networkInfoService)
        {
            _grooveMusicService = grooveMusicService;
            _lfClient = lfClient;
            _networkInfoService = networkInfoService;

            _artistsQueue = new TaskQueue();
            _albumsQueue = new TaskQueue();
        }

        public Task<string> CacheAndGetArtistImage(string artistsName)
        {
            return _artistsQueue.Enqueue(() => CacheAndGetArtistImageInternal(artistsName));
        }

        public Task<string> CacheAndGetAlbumImage(string trackTitle, string artistName)
        {
            return _albumsQueue.Enqueue(() => CacheAndGetAlbumImageInternal(trackTitle, artistName));
        }

        public Task<string> CacheAndGetAlbumImageUrl(string trackTitle, string thumbUrl)
        {
            return _albumsQueue.Enqueue(() => CacheAndGetAlbumImageUrlInternal(trackTitle, thumbUrl));
        }

        public async Task<string> GetCachedArtistImage(string artistName)
        {
            try
            {
                if (artistName == LibraryDatabaseService.UNKNOWN_ARTIST_NAME)
                    return null;

                string fileTitle = artistName.Length > 25 ? artistName.Remove(25) : artistName;
                var folder = await GetCreateFolder(ARTISTS_FOLDER_NAME);
                return await Get(fileTitle + ".jpg", folder);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string> GetCachedAlbumImage(string trackTitle)
        {
            try
            {
                string fileTitle = trackTitle.Length > 25 ? trackTitle.Remove(25) : trackTitle;
                var folder = await GetCreateFolder(ALBUMS_FOLDER_NAME);
                return await Get(fileTitle + ".jpg", folder);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IList<string>> GetCachedAlbumsImages(uint count)
        {
            try
            {
                var folder = await GetCreateFolder(ALBUMS_FOLDER_NAME);
                var files = await folder.GetFilesAsync(CommonFileQuery.DefaultQuery, 0, count);
                return files.Where(f => f.FileType == ".jpg").Select(f => f.Path).ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> ClearAlbumsCache()
        {
            try
            {
                var folder = await KnownFolders.PicturesLibrary.GetFolderAsync(ALBUMS_FOLDER_NAME);
                await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
                return true;
            }
            catch (Exception) { return false; }
        }
        
        public async Task<bool> ClearArtistsCache()
        {
            try
            {
                var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync(ARTISTS_FOLDER_NAME);
                await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
                return true;
            }
            catch (Exception) { return false; }
        }
        
        public async Task<FileSize> GetAlbumsCacheSize()
        {
            try
            {
                var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync(ALBUMS_FOLDER_NAME );
                var basic = await folder.GetBasicPropertiesAsync();
                IDictionary<string, object> properties = await folder.Properties.RetrievePropertiesAsync(new[] { "System.Size" });

                return FileSize.FromBytes((ulong)properties["System.Size"]);
            }
            catch (Exception) { return FileSize.FromBytes(0); }
        }
        
        public async Task<FileSize> GetArtistsCacheSize()
        {
            try
            {
                var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync(ARTISTS_FOLDER_NAME);
                var properties = await folder.GetBasicPropertiesAsync();
                return FileSize.FromBytes(properties.Size);
            }
            catch (Exception) { return FileSize.FromBytes(0); }
        }

        private async Task<string> CacheAndGetArtistImageInternal(string artistName)
        {
            string result = null;
            try
            {
                result = await GetCachedArtistImage(artistName);
            }
            catch (Exception) { }

            if (result != null)
                return result;

            if (!_networkInfoService.CanAppUseInternet)
                return null;

            string imageUrl = await _grooveMusicService.GetArtistImageURL(artistName);
            if (imageUrl == null)
            {
                var response = await _lfClient.Artist.GetInfoAsync(artistName, autocorrect: true);
                if (response.Success)
                {
                    try
                    {
                        imageUrl = response.Content.MainImage.Largest.ToString();
                    }
                    catch (Exception) { }
                }
            }

            if (imageUrl != null)
            {
                try
                {
                    string fileTitle = artistName.Length > 25 ? artistName.Remove(25) : artistName;
                    result = await CacheAndGet(
                        fileTitle, 
                        imageUrl, 
                        await GetCreateFolder(ARTISTS_FOLDER_NAME));
                }
                catch (Exception) { }
            }
            
            return result;
        }

        private async Task<string> CacheAndGetAlbumImageInternal(string trackTitle, string artistName)
        {
            string result = null;
            try
            {
                result = await GetCachedAlbumImage(trackTitle);
            }
            catch (Exception) { }

            if (result != null)
                return result;

            if (!_networkInfoService.CanAppUseInternet)
                return null;

            var response = await _lfClient.Track.GetInfoAsync(trackTitle, artistName, autocorrect: true);
            if (response.Success)
            {
                try
                {
                    if (response.Content.Images != null)
                    {
                        string fileTitle = trackTitle.Length > 25 ? trackTitle.Remove(25) : trackTitle;
                        result = await CacheAndGet(
                            trackTitle.Remove(25),
                            response.Content.Images.Largest.ToString(),
                            await GetCreateFolder(ALBUMS_FOLDER_NAME));
                    }
                }
                catch (Exception) { }
            }

            return result;
        }

        private async Task<string> CacheAndGetAlbumImageUrlInternal(string trackTitle, string thumbUrl)
        {
            string result = null;
            try
            {
                result = await GetCachedAlbumImage(trackTitle);
            }
            catch (Exception) { }

            if (result != null)
                return result;

            if (!_networkInfoService.CanAppUseInternet)
                return null;

            try
            {
                string fileTitle = trackTitle.Length > 25 ? trackTitle.Remove(25) : trackTitle;
                result = await CacheAndGet(
                    fileTitle,
                    thumbUrl,
                    await GetCreateFolder(ALBUMS_FOLDER_NAME));
            }
            catch (Exception) { }

            return result;
        }

        private async Task<StorageFolder> GetCreateFolder(string folderName)
        {
            return await ApplicationData.Current.LocalFolder.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);
        }
        
        private async Task<string> CacheAndGet(string name, string url, StorageFolder folder)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(new Uri(url));
                var file = await folder.CreateFileAsync($"{name}.temp", CreationCollisionOption.OpenIfExists);

                if ((await file.GetBasicPropertiesAsync()).Size <= 1024)
                    await FileIO.WriteBufferAsync(file, await response.Content.ReadAsBufferAsync());

                if ((await file.GetBasicPropertiesAsync()).Size <= 1024)
                {
                    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    return null;
                }

                await file.RenameAsync(name + ".jpg");
                return file.Path;
            }
        }
        
        private async Task<string> Get(string name, StorageFolder folder)
        {
            var file = await folder.GetFileAsync(name);
            if ((await file.GetBasicPropertiesAsync()).Size <= 1024)
                return null;

            return file.Path;
        }

        private readonly TaskQueue _artistsQueue;
        private readonly TaskQueue _albumsQueue;
        private readonly LastfmClient _lfClient;
        private readonly IGrooveMusicService _grooveMusicService;
        private readonly INetworkInfoService _networkInfoService;

        private const string ALBUMS_FOLDER_NAME = "VKSaver Albums";
        private const string ARTISTS_FOLDER_NAME = "VKSaver Artists";        
    }
}
