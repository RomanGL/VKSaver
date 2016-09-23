using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Database;
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

            lock (_lockObject)
            {
                _isUpdating = false;
            }
        }

        public async Task<List<VKSaverTrack>> GetAllTracks()
        {
            return await _database.GetItems<VKSaverTrack>();
        }

        public async Task<List<VKSaverArtist>> GetAllArtists()
        {
            return await _database.GetItems<VKSaverArtist>();
        }

        public async Task<VKSaverArtist> GetArtist(string dbKey)
        {
            return await _database.GetItemWithChildrens<VKSaverArtist>(dbKey);
        }

        private async Task UpdateDatabase()
        {
            _total = _artists.Count;
            _current = 0;
            OnPreparingDatabaseProgressChanged();

            foreach (VKSaverArtist artist in _artists.Values)
            {
                _current++;
                OnPreparingDatabaseProgressChanged();
                await _database.UpdateItemChildrens(artist);
            }

            _settingsService.Set(AppConstants.CURRENT_LIBRARY_INDEX_PARAMETER, AppConstants.CURRENT_LIBRARY_INDEX);
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
                }

                _current++;

                OnSearchingFilesProgressChanged();
            }

            await _database.InsertItems(tracks);
        }

        private async Task<VKSaverTrack> GetTrackInfo(StorageFile file)
        {
            try
            {
                if (file.FileType == ".mp3")
                    return await ProcessMp3File(file);
                else if (file.FileType == MusicCacheService.FILES_EXTENSION) // Кэшированный файл ВКачай.
                    return await ProcessVKSaverAudioFile(file);
                else
                    return null;
            }       
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<VKSaverTrack> ProcessMp3File(StorageFile file)
        {
            var properties = await file.Properties.GetMusicPropertiesAsync();

            var track = new VKSaverTrack();
            track.Source = file.Path;

            if (String.IsNullOrWhiteSpace(properties.Title))
                track.Title = file.DisplayName;
            else
                track.Title = properties.Title;

            if (!String.IsNullOrWhiteSpace(properties.Artist))
                ProcessArtist(track, properties.Artist);
            else
                ProcessArtist(track, "Unknown");

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
                    Title = metadata.Track.Title
                };

                ProcessArtist(track, metadata.Track.Artist);
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
            
            artist.Tracks.Add(track);
        }

        private void ClearUpdateTemp()
        {
            _artists.Clear();
        }

        private void OnSearchingFilesProgressChanged()
        {
            if (UpdateProgressChanged != null)
            {
                UpdateProgressChanged(this, new DBUpdateProgressChangedEventArgs
                {
                    Step = DatabaseUpdateStepType.SearchingFiles,
                    TotalItems = _total,
                    CurrentItem = _current
                });
            }
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

        private readonly Dictionary<string, VKSaverArtist> _artists = new Dictionary<string, VKSaverArtist>();
        private readonly object _lockObject = new object();        
    }
}
