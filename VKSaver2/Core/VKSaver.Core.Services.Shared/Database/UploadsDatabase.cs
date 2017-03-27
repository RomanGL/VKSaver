#if ANDROID
using SQLite.Net.Platform.XamarinAndroid;
#endif

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Transfer;
using SQLite.Net.Attributes;
using SQLite.Net;
using SQLite.Net.Interop;

namespace VKSaver.Core.Services.Database
{
    internal sealed class UploadsDatabase : VKSaverDatabase
    {
        private UploadsDatabase(string databasePath) 
            : base(databasePath)
        {
        }

        public Task InsertAsync(ICompletedUpload upload)
        {
            return Task.Run(() =>
            {
                using (var db = new SQLiteConnection(_platform, _databasePath))
                {
                    CreateTableIfNotExist<UploadDatabaseItem>(db);

                    var item = new UploadDatabaseItem
                    {
                        Id = upload.Id,
                        Name = upload.Name,
                        ContentType = upload.ContentType,
                        ServerResponse = upload.ServerResponse
                    };

                    db.Insert(item);
                    db.Close();
                }                
            });
        }

        public Task InsertOrReplaceAsync(ICompletedUpload upload)
        {
            return Task.Run(() =>
            {
                using (var db = new SQLiteConnection(_platform, _databasePath))
                {
                    CreateTableIfNotExist<UploadDatabaseItem>(db);

                    var item = new UploadDatabaseItem
                    {
                        Id = upload.Id,
                        Name = upload.Name,
                        ContentType = upload.ContentType,
                        ServerResponse = upload.ServerResponse
                    };

                    db.InsertOrReplace(item);
                    db.Close();
                }
            });
        }

        public Task<ICompletedUpload> GetAsync(Guid guid)
        {
            return Task.Run<ICompletedUpload>(() =>
            {
                using (var db = new SQLiteConnection(_platform, _databasePath))
                {
                    CreateTableIfNotExist<UploadDatabaseItem>(db);

                    var item = db.Get<UploadDatabaseItem>(guid);
                    if (item == null)
                        return null;

                    return new CompletedUpload
                    {
                        Id = item.Id,
                        Name = item.Name,
                        ContentType = item.ContentType,
                        ServerResponse = item.ServerResponse
                    };
                }
            });
        }

        public Task<IEnumerable<ICompletedUpload>> GetNotCompletedAsync()
        {
            return Task.Run<IEnumerable<ICompletedUpload>>(() =>
            {
                using (var db = new SQLiteConnection(_platform, _databasePath))
                {
                    CreateTableIfNotExist<UploadDatabaseItem>(db);

                    var result = from upload in db.Table<UploadDatabaseItem>()
                                 where upload.ServerResponse != null
                                 select new CompletedUpload
                                 {
                                     Id = upload.Id,
                                     Name = upload.Name,
                                     ContentType = upload.ContentType,
                                     ServerResponse = upload.ServerResponse
                                 };
                    return result.ToList();
                }
            });
        }

        public Task RemoveAsync(Guid guid)
        {
            return Task.Run(() =>
            {
                using (var db = new SQLiteConnection(_platform, _databasePath))
                {
                    CreateTableIfNotExist<UploadDatabaseItem>(db);
                    db.Delete<UploadDatabaseItem>(guid);
                }
            });
        }

        public static async Task<UploadsDatabase> CreateDatabaseAsync()
        {
            string path = await GetDatabasePathAsync(DATABASE_NAME);
            if (path == null)
                throw new InvalidOperationException("Не удалось получить путь к файлу базы данных.");

            return new UploadsDatabase(path);
        }

#if WINDOWS_UWP || WINDOWS_PHONE_APP
        private readonly ISQLitePlatform _platform = new SQLitePlatformZ();
#else
        private readonly ISQLitePlatform _platform = new SQLitePlatformAndroid();
#endif

        private const string DATABASE_NAME = "uploads.db";

        private class UploadDatabaseItem
        {
            [PrimaryKey]
            public Guid Id { get; set; }

            public string Name { get; set; }
            
            public FileContentType ContentType { get; set; }

            public string ServerResponse { get; set; }
        }
    }
}
