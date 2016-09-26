using ModernDev.InTouch;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Database;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Database;
using VKSaver.Core.Services.Interfaces;
using Windows.Storage;
using Windows.Storage.Search;

namespace VKSaver.Core.Services
{
    public sealed class LibraryDatabaseService : ILibraryDatabaseService
    {
        public event TypedEventHandler<ILibraryDatabaseService, DBUpdateProgressChangedEventArgs> UpdateProgressChanged;

        public LibraryDatabaseService(
            LibraryDatabase database, 
            ISettingsService settingsService,
            ILogService logService)
        {
            _database = database;
            _settingsService = settingsService;
            _logService = logService;
        }

        public async void Update()
        {
            try
            {
                lock (_lockObject)
                {
                    if (_isUpdating)
                        return;
                    _isUpdating = true;
                }

                UpdateProgressChanged?.Invoke(this, new DBUpdateProgressChangedEventArgs { Step = DatabaseUpdateStepType.Started });

                await _database.ClearDatabase();
                await _database.Initialize();

                await LoadFiles();
                await UpdateDatabase();
                ClearUpdateTemp();

                UpdateProgressChanged?.Invoke(this, new DBUpdateProgressChangedEventArgs { Step = DatabaseUpdateStepType.Completed });
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);

                _database.CloseConnection();
                var dbFile = await ApplicationData.Current.LocalFolder.GetFileAsync(LibraryDatabase.DATABASE_FILE_NAME);
                await dbFile.DeleteAsync(StorageDeleteOption.PermanentDelete);

                UpdateProgressChanged?.Invoke(this, new DBUpdateProgressChangedEventArgs { Step = DatabaseUpdateStepType.Completed });
            }
            finally
            {
                lock (_lockObject)
                {
                    _isUpdating = false;
                }
            }
        }

        public bool NeedReloadLibraryView { get; set; }

        public async Task<List<VKSaverTrack>> GetAllTracks()
        {
            return await _database.GetItems<VKSaverTrack>();
        }

        public async Task<List<VKSaverArtist>> GetAllArtists()
        {
            return await _database.GetItems<VKSaverArtist>();
        }

        public async Task<List<VKSaverAlbum>> GetAllAlbums()
        {
            return await _database.GetItems<VKSaverAlbum>();
        }

        public async Task<List<VKSaverGenre>> GetAllGenres()
        {
            return await _database.GetItems<VKSaverGenre>();
        }

        public async Task<List<VKSaverFolder>> GetAllFolders()
        {
            return await _database.GetItems<VKSaverFolder>();
        }

        public async Task<List<VKSaverTrack>> GetAllCachedTracks()
        {
            return await _database.GetItems<VKSaverTrack>(t => t.VKInfoKey != null);
        }

        public async Task<VKSaverArtist> GetArtist(string dbKey)
        {
            return await _database.GetItemWithChildrens<VKSaverArtist>(dbKey);
        }

        public async Task<VKSaverAlbum> GetAlbum(string dbKey)
        {
            return await _database.GetItemWithChildrens<VKSaverAlbum>(dbKey);
        }

        public async Task<VKSaverGenre> GetGenre(string dbKey)
        {
            return await _database.GetItemWithChildrens<VKSaverGenre>(dbKey);
        }

        public async Task<VKSaverFolder> GetFolder(string dbKey)
        {
            return await _database.GetItemWithChildrens<VKSaverFolder>(dbKey);
        }

        public async Task RemoveItem<T>(T item)
        {
            await _database.RemoveItem(item);
            NeedReloadLibraryView = true;
        }

        public async Task RemoveItemByPrimaryKey<T>(object primaryKey)
        {
            await _database.RemoveItem<T>(primaryKey);
            NeedReloadLibraryView = true;
        }

        private async Task UpdateDatabase()
        {
            _total = _folders.Count + _artists.Count + _albums.Count + _genres.Count + _vksmInfo.Count + 5;
            _current = 0;
            OnPreparingDatabaseProgressChanged();

            _current++;
            OnPreparingDatabaseProgressChanged();
            await _database.InsertItems(_folders.Values);

            _current++;
            OnPreparingDatabaseProgressChanged();
            await _database.InsertItems(_artists.Values);

            _current++;
            OnPreparingDatabaseProgressChanged();
            await _database.InsertItems(_albums.Values);

            _current++;
            OnPreparingDatabaseProgressChanged();
            await _database.InsertItems(_genres.Values);

            _current++;
            OnPreparingDatabaseProgressChanged();
            await _database.InsertItems(_vksmInfo);

            await UpdateDatabaseChildrens(_folders.Values);
            await UpdateDatabaseChildrens(_artists.Values);
            await UpdateDatabaseChildrens(_albums.Values);
            await UpdateDatabaseChildrens(_genres.Values);
            await UpdateDatabaseChildrens(_vksmInfo);

            _settingsService.Set(AppConstants.CURRENT_LIBRARY_INDEX_PARAMETER, AppConstants.CURRENT_LIBRARY_INDEX);
        }

        private async Task UpdateDatabaseChildrens<T>(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                _current++;
                OnPreparingDatabaseProgressChanged();
                await _database.UpdateItemChildrens(item);
            }
        }

        private async Task LoadFiles()
        {
            _current = 0;
            _total = 0;
            await RecursiveScanFolder(KnownFolders.MusicLibrary);
        }

        private async Task RecursiveScanFolder(StorageFolder folder)
        {
            var folders = await folder.GetFoldersAsync();
            foreach (var childFolder in folders)
            {
                await RecursiveScanFolder(childFolder);
            }

            VKSaverFolder zFolder = null;
            if (!_folders.TryGetValue(folder.Path, out zFolder))
            {
                zFolder = new VKSaverFolder
                {
                    Name = folder.DisplayName,
                    Path = String.IsNullOrEmpty(folder.Path) ? 
                    MusicFilesPathHelper.CAPATIBLE_MUSIC_FOLDER_NAME : 
                    MusicFilesPathHelper.GetCapatibleSource(folder.Path),
                    Tracks = new List<VKSaverTrack>()
                };
                _folders[zFolder.Path] = zFolder;
            }

            var files = await folder.GetFilesAsync(CommonFileQuery.DefaultQuery);
            _total += files.Count;

            OnSearchingFilesProgressChanged();

            var tracks = new List<VKSaverTrack>();
            foreach (var file in files)
            {
                var track = await GetTrackInfo(file);
                if (track != null)
                {
                    tracks.Add(track);
                    zFolder.Tracks.Add(track);
                }

                _current++;
                OnSearchingFilesProgressChanged();
            }

            await _database.InsertItems(tracks);
        }

        private async Task<VKSaverTrack> GetTrackInfo(StorageFile file)
        {
            VKSaverTrack track = null;

            try
            {
                if (file.FileType == ".mp3")
                    track = await ProcessMp3File(file);
                else if (file.FileType == MusicCacheService.FILES_EXTENSION) // Кэшированный файл ВКачай.
                    track = await ProcessVKSaverAudioFile(file);
            }
            catch (Exception) { }

            if (track != null)
                 track.Source = MusicFilesPathHelper.GetCapatibleSource(file.Path);

            return track;
        }

        private async Task<VKSaverTrack> ProcessMp3File(StorageFile file)
        {
            var properties = await file.Properties.GetMusicPropertiesAsync();

            var track = new VKSaverTrack();
            track.Source = file.Path;

            if (String.IsNullOrWhiteSpace(properties.Title))
                track.Title = file.DisplayName.Trim();
            else
                track.Title = properties.Title.Trim();

            if (!String.IsNullOrWhiteSpace(properties.Artist))
                ProcessArtist(track, properties.Artist.Trim());
            else
                ProcessArtist(track, "Unknown");

            if (!String.IsNullOrWhiteSpace(properties.Album))
                ProcessAlbum(track, properties.Album.Trim());
            else
                ProcessAlbum(track, "Unknown");

            if (properties.Genre.Count > 0)
            {
                foreach (string genre in properties.Genre)
                    ProcessGenre(track, genre.Trim());
            }
            else
                ProcessGenre(track, "Unknown");

            track.Duration = properties.Duration;
            return track;
        }

        private async Task<VKSaverTrack> ProcessVKSaverAudioFile(StorageFile file)
        {
            using (var audioFile = new VKSaverAudioFile(file))
            {
                var metadata = await audioFile.GetMetadataAsync();
                var track = new VKSaverTrack
                {
                    Source = file.Path,
                    Title = metadata.Track.Title.Trim()
                };

                ProcessArtist(track, metadata.Track.Artist.Trim());
                ProcessAlbum(track, "Unknown");
                ProcessGenre(track, metadata.VK.Genre == (AudioGenres)0 ? "Unknown" : metadata.VK.Genre.ToString());
                ProcessVKInfo(track, metadata.VK);

                track.Duration = TimeSpan.FromTicks(metadata.Track.Duration);
                return track;
            }
        }

        private void ProcessArtist(VKSaverTrack track, string artistName)
        {
            VKSaverArtist artist = null;
            if (!_artists.TryGetValue(artistName, out artist))
            {
                artist = new VKSaverArtist
                {
                    DbKey = artistName,
                    Name = artistName,
                    Tracks = new List<VKSaverTrack>()
                };
                _artists[artistName] = artist;
            }

            track.Artist = artistName;
            artist.Tracks.Add(track);
        }

        private void ProcessGenre(VKSaverTrack track, string genreName)
        {
            VKSaverGenre genre = null;
            if (!_genres.TryGetValue(genreName, out genre))
            {
                genre = new VKSaverGenre
                {
                    DbKey = genreName,
                    Name = genreName,
                    Tracks = new List<VKSaverTrack>()
                };
                _genres[genreName] = genre;
            }

            genre.Tracks.Add(track);
        }

        private void ProcessAlbum(VKSaverTrack track, string albumName)
        {
            string dbKey = $"{track.Artist}-{albumName}";

            VKSaverAlbum album = null;
            if (!_albums.TryGetValue(dbKey, out album))
            {
                album = new VKSaverAlbum
                {
                    DbKey = dbKey,
                    Name = albumName,
                    Tracks = new List<VKSaverTrack>()
                };
                _albums[dbKey] = album;
            }

            VKSaverArtist artist = _artists[track.Artist];

            if (artist.Albums == null)
                artist.Albums = new List<VKSaverAlbum>();

            if (artist.Albums.FirstOrDefault(a => a.DbKey == dbKey) == null)
                artist.Albums.Add(album);

            album.Tracks.Add(track);
        }

        private void ProcessVKInfo(VKSaverTrack track, VKSaverAudioVKInfo info)
        {
            info.DbKey = $"{info.OwnerID} {info.ID}";
            track.VKInfoKey = info.DbKey;
            _vksmInfo.Add(info);
        }

        private void ClearUpdateTemp()
        {
            _artists.Clear();
            _albums.Clear();
            _folders.Clear();
            _genres.Clear();
            _vksmInfo.Clear();
        }

        private void OnSearchingFilesProgressChanged()
        {
            UpdateProgressChanged?.Invoke(this, new DBUpdateProgressChangedEventArgs
            {
                Step = DatabaseUpdateStepType.SearchingFiles,
                TotalItems = _total,
                CurrentItem = _current
            });
        }

        private void OnPreparingDatabaseProgressChanged()
        {
            if (UpdateProgressChanged != null)
            {
                UpdateProgressChanged(this, new DBUpdateProgressChangedEventArgs
                {
                    Step = DatabaseUpdateStepType.PreparingDatabase,
                    TotalItems = _total,
                    CurrentItem = _current
                });
            }
        }

        private int _current;
        private int _total;
        private bool _isUpdating;

        private readonly LibraryDatabase _database;
        private readonly ISettingsService _settingsService;
        private readonly ILogService _logService;

        private readonly Dictionary<string, VKSaverFolder> _folders = new Dictionary<string, VKSaverFolder>();
        private readonly Dictionary<string, VKSaverArtist> _artists = new Dictionary<string, VKSaverArtist>();
        private readonly Dictionary<string, VKSaverAlbum> _albums = new Dictionary<string, VKSaverAlbum>();
        private readonly Dictionary<string, VKSaverGenre> _genres = new Dictionary<string, VKSaverGenre>();
        private readonly List<VKSaverAudioVKInfo> _vksmInfo = new List<VKSaverAudioVKInfo>();

        private readonly object _lockObject = new object();        
    }
}
