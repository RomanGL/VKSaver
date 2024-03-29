﻿using SQLite.Net;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace VKSaver.Core.Services.Database
{
    internal abstract class VKSaverDatabase
    {
        protected VKSaverDatabase(string databasePath)
        {
            if (String.IsNullOrEmpty(databasePath))
                throw new ArgumentException(nameof(databasePath));

            _databasePath = databasePath;
        }
        
        protected void CreateTableIfNotExist<T>(SQLiteConnection db)
        {
            var tableInfo = db.GetTableInfo(typeof(T).Name);
            if (!tableInfo.Any())
                db.CreateTable<T>();
        }

        protected static async Task<string> GetDatabasePathAsync(string databaseName)
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    databaseName, CreationCollisionOption.OpenIfExists);
                return file.Path;
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected readonly string _databasePath;
    }
}
