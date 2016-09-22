using SQLite.Net;
using SQLite.Net.Interop;
using SQLiteNetExtensions.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public Task UpdateItemChildrens(object item)
        {
            return Task.Run(() =>
            {
                var conn = GetDbConnection();
                conn.UpdateWithChildren(item);
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

        public void UpdateItem(object item)
        {
            var conn = GetDbConnection();
            conn.Update(item);
        }

        private SQLiteConnection GetDbConnection()
        {
            lock (_lockObject)
            {
                if (_connection == null)
                {
                    var pl = new SQLitePlatformZ();
                    _connection = new SQLiteConnection(pl, DATABASE_FILE_NAME, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
                }

                return _connection;
            }
        }

        private SQLiteConnection _connection;
        private readonly object _lockObject = new object();

        private const string DATABASE_FILE_NAME = "library.db";
    }
}
