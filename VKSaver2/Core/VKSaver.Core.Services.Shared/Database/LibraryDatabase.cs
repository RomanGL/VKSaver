using SQLite.Net;
using SQLite.Net.Interop;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Database;

namespace VKSaver.Core.Services.Database
{
    public sealed class LibraryDatabase
    {
        public LibraryDatabase()
        {

        }

        public async Task Initialize()
        {
            await Task.Run(() =>
            {
                var conn = GetDbConnection();

                conn.CreateTable<VKSaverTrack>();
                conn.CreateTable<VKSaverArtist>();
                conn.CreateTable<VKSaverAudioVKInfo>();
                conn.CreateTable<VKSaverAlbum>();
                conn.CreateTable<VKSaverGenre>();
                conn.CreateTable<VKSaverFolder>();
            });
        }

        public async Task Deinitialize()
        {
            await Task.Run(() =>
            {
                var conn = GetDbConnection();
                conn.Close();
                _connection = null;
            });
        }

        public async Task ClearDatabase()
        {
            await Task.Run(() =>
            {
                var conn = GetDbConnection();

                conn.DropTable<VKSaverTrack>();
                conn.DropTable<VKSaverArtist>();
                conn.DropTable<VKSaverAudioVKInfo>();
                conn.DropTable<VKSaverAlbum>();
                conn.DropTable<VKSaverGenre>();
                conn.DropTable<VKSaverFolder>();
            });
        }

        public Task InsertItems(IEnumerable<object> items)
        {
            return Task.Run(() =>
            {
                var conn = GetDbConnection();
                conn.InsertAll(items);
            });
        }

        public Task InsertItem(object item)
        {
            return Task.Run(() =>
            {
                var conn = GetDbConnection();
                conn.Insert(item);
            });
        }

        public Task UpdateItemChildrens(object item)
        {
            return Task.Run(() =>
            {
                var conn = GetDbConnection();
                conn.UpdateWithChildren(item);
            });
        }

        public Task<T> FindItem<T>(object primaryKey) where T : class
        {
            return Task.Run(() =>
            {
                var conn = GetDbConnection();
                return conn.Find<T>(primaryKey);
            });
        }

        public Task<T> FindItem<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return Task.Run(() =>
            {
                var conn = GetDbConnection();
                return conn.Find<T>(predicate);
            });
        }

        public async Task<List<T>> GetItems<T>() where T : class
        {
            return await Task.Run(() =>
            {
                var conn = GetDbConnection();
                return conn.Table<T>().ToList();
            });
        }

        public Task<List<T>> GetItems<T>(Func<T, bool> selector) where T : class
        {
            return Task.Run(() =>
            {
                var conn = GetDbConnection();
                return conn.Table<T>().Where(selector).ToList();
            });
        }

        public async Task<T> GetItemWithChildrens<T>(object primaryKey) where T : class
        {
            return await Task.Run(() =>
            {
                var conn = GetDbConnection();
                return conn.GetWithChildren<T>(primaryKey);
            });
        }

        public async Task UpdateItems(IEnumerable<object> items)
        {
            await Task.Run(() =>
            {
                var conn = GetDbConnection();
                conn.DeleteAll(items);
                conn.InsertAll(items);
            });
        }

        public Task RemoveItem<T>(T item)
        {
            return Task.Run(() =>
            {
                var conn = GetDbConnection();
                conn.Delete(item);
            });
        }

        public Task RemoveItem<T>(object primaryKey)
        {
            return Task.Run(() =>
            {
                var conn = GetDbConnection();
                conn.Delete<T>(primaryKey);
            });
        }

        public void UpdateItem(object item)
        {
            var conn = GetDbConnection();
            conn.Update(item);
        }

        public void SetMaxVariablesLimit()
        {
            var conn = GetDbConnection();
            _platform.VKSaverSQLiteApi.SetLimit(conn.Handle, 9, 5000);
        }

        public void CloseConnection()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }

        public SQLiteConnection GetDbConnection()
        {
            lock (_lockObject)
            {
                if (_connection == null)
                {
                    _platform = new SQLitePlatformZ();
                    _connection = new SQLiteConnection(_platform, DATABASE_FILE_NAME, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
                }

                return _connection;
            }
        }

        private SQLiteConnection _connection;
        private SQLitePlatformZ _platform;
        private readonly object _lockObject = new object();

        public const string DATABASE_FILE_NAME = "library.db";
    }
}
