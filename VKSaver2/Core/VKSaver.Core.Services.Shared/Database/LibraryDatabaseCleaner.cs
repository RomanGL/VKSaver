using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Database;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services.Database
{
    public sealed class LibraryDatabaseCleaner
    {
        internal LibraryDatabaseCleaner(LibraryDatabase database, ILibraryDatabaseService libraryDatabaseService)
        {
            _database = database;
            _libraryDatabaseService = libraryDatabaseService;
        }

        /// <summary>
        /// Удаляет элемент из базы данных и очищает все зависимости.
        /// Возвращает коллекцию элементов, которые также были удалены.
        /// </summary>
        public async Task<List<object>> RemoveItemAndCleanDependenciesAsync(object item)
        {
            if (item is VKSaverTrack)
                return await RemoveTrack((VKSaverTrack)item);

            return new List<object>();
        }

        private async Task<List<object>> RemoveTrack(VKSaverTrack track)
        {
            var result = new List<object>();

            var genre = await _database.GetItemWithChildrens<VKSaverGenre>(track.GenreKey);
            if (genre.Tracks.Count == 1)
            {
                result.Add(genre);
                await _database.RemoveItem(genre);
            }

            var folder = await _database.GetItemWithChildrens<VKSaverFolder>(track.FolderKey);
            if (folder.Tracks.Count == 1)
            {
                result.Add(folder);
                await _database.RemoveItem(folder);
            }

            var album = await _database.GetItemWithChildrens<VKSaverAlbum>(track.AlbumKey);
            if (album.Tracks.Count == 1)
            {
                var artist = await _database.GetItemWithChildrens<VKSaverArtist>(album.ArtistName);
                if (artist.Albums.Count == 1)
                {
                    result.Add(artist);
                    await _database.RemoveItem(artist);
                }

                result.Add(album);
                await _database.RemoveItem(album);
            }

            if (track.VKInfoKey != null)
            {
                await _database.RemoveItem<VKSaverAudioVKInfo>(track.VKInfoKey);
            }

            result.Add(track);
            await _database.RemoveItem(track);

            _libraryDatabaseService.NeedReloadLibraryView = true;
            return result;
        }

        private async Task<List<object>> RemoveArtist(VKSaverArtist artist)
        {
            var result = new List<object>();

            var dbArtist = await _database.GetItemWithChildrens<VKSaverArtist>(artist.DbKey);
            
            var genresDict = new Dictionary<string, VKSaverGenre>();
            var foldersDict = new Dictionary<string, VKSaverFolder>();
            var vkInfoDict = new Dictionary<string, VKSaverAudioVKInfo>();

            foreach (var track in dbArtist.Tracks)
            {
                genresDict[track.GenreKey] = track.AppGenre;
                foldersDict[track.FolderKey] = track.AppFolder;

                if (track.VKInfoKey != null)
                    vkInfoDict[track.VKInfoKey] = track.VKInfo;

                await _database.RemoveItem(track);
                result.Add(track);
            }

            result.AddRange(genresDict.Values);
            result.AddRange(foldersDict.Values);
            result.AddRange(vkInfoDict.Values);

            await RemoveItemsByKey<VKSaverGenre>(genresDict.Keys);
            await RemoveItemsByKey<VKSaverFolder>(foldersDict.Keys);
            await RemoveItemsByKey<VKSaverAudioVKInfo>(vkInfoDict.Keys);

            foreach (var album in dbArtist.Albums)
            {
                result.Add(album);
                await _database.RemoveItem(album);
            }

            result.Add(artist);
            await _database.RemoveItem(artist);

            return result;
        }

        private async Task RemoveItemsByKey<T>(IEnumerable<object> keys) where T : class
        {
            foreach (var key in keys)
            {
                await _database.RemoveItem<T>(key);
            }
        }

        private readonly LibraryDatabase _database;
        private readonly ILibraryDatabaseService _libraryDatabaseService;
    }
}
