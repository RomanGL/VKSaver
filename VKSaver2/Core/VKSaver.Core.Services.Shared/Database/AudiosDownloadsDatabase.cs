using System;
using System.Threading.Tasks;
using SQLite.Net;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Services.Database
{
    internal sealed class AudiosDownloadsDatabase : VKSaverDatabase
    {
        private AudiosDownloadsDatabase(string databasePath) 
            : base(databasePath)
        {
        }

        public Task InsertAsync(VKSaverAudio item, Guid guid)
        {
            return Task.Run(() =>
            {
                using (var db = new SQLiteConnection(_platform, _databasePath))
                {
                    CreateTableIfNotExist<DownloadDatabaseItem>(db);
                    db.Insert(item);
                    db.Close();
                }
            });
        }

        public Task InsertOrReplaceAsync(VKSaverAudio item, Guid guid)
        {
            return Task.Run(() =>
            {
                using (var db = new SQLiteConnection(_platform, _databasePath))
                {
                    CreateTableIfNotExist<DownloadDatabaseItem>(db);
                    db.InsertOrReplace(item);
                    db.Close();
                }
            });
        }

        public Task<VKSaverAudio> GetAsync(Guid guid)
        {
            return Task.Run(() =>
            {
                using (var db = new SQLiteConnection(_platform, _databasePath))
                {
                    CreateTableIfNotExist<DownloadDatabaseItem>(db);

                    var item = db.Get<DownloadDatabaseItem>(guid);
                    return (VKSaverAudio)null;
                }
            });
        }

        public Task RemoveAsync(Guid guid)
        {
            return Task.Run(() =>
            {
                using (var db = new SQLiteConnection(_platform, _databasePath))
                {
                    CreateTableIfNotExist<DownloadDatabaseItem>(db);
                    db.Delete<DownloadDatabaseItem>(guid);
                }
            });
        }

        public static async Task<AudiosDownloadsDatabase> CreateDatabaseAsync()
        {
            string path = await GetDatabasePathAsync(DATABASE_NAME);
            if (path == null)
                throw new InvalidOperationException("Не удалось получить путь к файлу базы данных.");

            return new AudiosDownloadsDatabase(path);
        }

        private readonly SQLitePlatformZ _platform = new SQLitePlatformZ();
        private const string DATABASE_NAME = "downloads.db";

        private sealed class DownloadDatabaseItem
        {
            [PrimaryKey]
            public Guid Id { get; set; }

            public string Name { get; set; }

            [ForeignKey(typeof(VKSaverAudioVKInfo))]
            public string VKInfoKey { get; set; }

            [ForeignKey(typeof(VKSaverAudioTrackInfo))]
            public string AudioTrackInfoKey { get; set; }
        }
    }
}
