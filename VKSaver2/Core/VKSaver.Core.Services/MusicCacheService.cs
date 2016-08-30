using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Interfaces;
using Windows.Storage;
using Windows.Storage.Streams;

namespace VKSaver.Core.Services
{
    public sealed class MusicCacheService : IMusicCacheService
    {
        public Task<bool> ConvertAudioToVKSaverFormat(StorageFile file)
        {
            return _toVKSaverFormat.Enqueue(() => ConvertAudioToVKSaverFormatInternal(file));
        }
                
        public async Task<Stream> GetCachedFileStream(string fileName)
        {
            try
            {
                var file = await GetCachedFile(fileName);
                var fileStream = await file.OpenStreamForReadAsync();
                return new MusicEncryptedStream(fileStream);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task<bool> ConvertAudioToVKSaverFormatInternal(StorageFile file)
        {
            bool result = false;
            string fileName = file.Name.Split(new char[] { '.' })[0] + FILES_EXTENSION;

            Stream readStream = null;
            MusicEncryptedStream writeStream = null;
            StorageFile convertedFile = null;

            try
            {
                convertedFile = await (await GetCacheFolder()).CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

                readStream = await file.OpenStreamForReadAsync();
                writeStream = new MusicEncryptedStream(await convertedFile.OpenStreamForWriteAsync());

                int readed = 0;
                var buffer = new byte[4096];

                do
                {
                    readed = readStream.Read(buffer, 0, 4096);
                    if (readed == 0)
                        continue;

                    writeStream.Write(buffer, 0, readed);

                } while (readed > 0);

                result = readStream.Position == writeStream.Position;
                writeStream.Flush();
            }
            catch (Exception) { }
            finally
            {
                readStream?.Dispose();
                writeStream?.Dispose();

                await file?.DeleteAsync(StorageDeleteOption.PermanentDelete);
                if (!result)
                    await convertedFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }

            return result;
        }

        private static async Task<StorageFile> GetCachedFile(string fileName)
        {
            var cacheFolder = await GetCacheFolder();
            return await cacheFolder.GetFileAsync(fileName);
        }

        private static async Task<StorageFolder> GetCacheFolder()
        {
            return await KnownFolders.MusicLibrary.CreateFolderAsync(
                MUSIC_CACHE_FOLDER_NAME, CreationCollisionOption.OpenIfExists);
        }

        private readonly TaskQueue _toVKSaverFormat = new TaskQueue();

        internal const string FILES_PROTECTION_PASSWORD = "VktTYXZlciAy";
        internal const string FILES_METADATA_NAME = "metadata.vks";
        internal const string FILES_CONTENT_NAME = "content.vks";
        internal const string FILES_EXTENSION = ".vksm";
        internal const string MUSIC_CACHE_FOLDER_NAME = "VKSaver";
    }
}
