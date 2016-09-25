using OneTeam.SDK.Core;
using OneTeam.SDK.LastFm.Models.Response;
using OneTeam.SDK.LastFm.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VKSaver.Core.Models;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Interfaces;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Web.Http;

namespace VKSaver.Core.Services
{
    /// <summary>
    /// Представляет сервис кэширования изображений.
    /// </summary>
    public sealed class ImagesCacheService : IImagesCacheService
    {    
        public ImagesCacheService(IGrooveMusicService grooveMusicService, ILFService lfService)
        {
            _grooveMusicService = grooveMusicService;
            _lfService = lfService;

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

        public async Task<string> GetCachedArtistImage(string artistName)
        {
            try
            {
                if (artistName == "Unknown")
                    return null;

                var folder = await GetCreateFolder(ARTISTS_FOLDER_NAME);
                return await Get(artistName, folder);
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
                var folder = await GetCreateFolder(ALBUMS_FOLDER_NAME);
                return await Get(trackTitle, folder);
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
                return files.Select(f => f.Path).ToList();
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
                var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync(ALBUMS_FOLDER_NAME);
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

            string imageUrl = await _grooveMusicService.GetArtistImageURL(artistName);
            if (imageUrl == null)
            {
                var parameters = new Dictionary<string, string>
                {
                    { "artist", artistName },
                    { "autocorrect", "1" }
                };

                var request = new Request<LFArtistInfoResponse>("artist.getInfo", parameters);
                var response = await _lfService.ExecuteRequestAsync(request);

                if (response.IsValid())
                {
                    imageUrl = response.Artist?.MegaImage?.URL;
                    if (!String.IsNullOrEmpty(imageUrl))
                    {
                        try
                        {
                            result = await CacheAndGet(
                                artistName,
                                imageUrl,
                                await GetCreateFolder(ALBUMS_FOLDER_NAME));
                        }
                        catch (Exception) { }
                    }
                }
            }

            if (imageUrl != null)
            {
                try
                {
                    result = await CacheAndGet(
                        artistName, 
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

            var parameters = new Dictionary<string, string>
            {
                { "track", trackTitle },
                { "artist", artistName },
                { "autocorrect", "1" }
            };

            var request = new Request<LFTrackInfoResponse>("track.getInfo", parameters);
            var response = await _lfService.ExecuteRequestAsync(request);

            if (response.IsValid() && response.Track.Album != null)
            {
                string imageUrl = response.Track.Album.MaxImage.URL;
                if (!String.IsNullOrEmpty(imageUrl))
                {
                    try
                    {
                        result = await CacheAndGet(
                            trackTitle,
                            imageUrl,
                            await GetCreateFolder(ALBUMS_FOLDER_NAME));
                    }
                    catch (Exception) { }
                }
            }

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

                await file.RenameAsync(name);
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
        private readonly IGrooveMusicService _grooveMusicService;
        private readonly ILFService _lfService;

        private const string ALBUMS_FOLDER_NAME = "AlbumsCache";
        private const string ARTISTS_FOLDER_NAME = "ArtistsCache";
    }
}
