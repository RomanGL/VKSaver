using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
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
        public async Task<bool> ConvertAudioToVKSaverFormat(StorageFile file, VKSaverAudio metadata)
        {
            if (metadata == null)
                return false;

            StorageFile zipFile = null;
            Stream zipFileStream = null;
            Stream fileStream = null;

            try
            {
                var cacheFolder = await GetCacheFolder();
                zipFile = await cacheFolder.CreateFileAsync(file.Name.Split(new char[] { '.' })[0] + FILES_EXTENSION, 
                    CreationCollisionOption.ReplaceExisting);
                zipFileStream = (await zipFile.OpenAsync(FileAccessMode.ReadWrite)).AsStream();
                fileStream = (await file.OpenAsync(FileAccessMode.Read)).AsStreamForRead();

                using (var zipStream = new ZipOutputStream(zipFileStream))
                {
                    zipStream.SetLevel(0);

                    var contentEntry = new ZipEntry(FILES_CONTENT_NAME);
                    contentEntry.CompressionMethod = CompressionMethod.Stored;
                    WriteEntry(zipStream, contentEntry, fileStream);

                    var metadataEntry = new ZipEntry(FILES_METADATA_NAME);
                    metadataEntry.CompressionMethod = CompressionMethod.Stored;
                    WriteEntry(zipStream, metadataEntry, new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metadata))));
                    
                    zipStream.Finish();
                }

                return true;
            }
            catch (Exception ex)
            {
                await zipFile?.DeleteAsync(StorageDeleteOption.PermanentDelete);
                return false;
            }
            finally
            {
                fileStream?.Dispose();
                zipFileStream?.Dispose();                
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }
                
        public async Task<CachedFileData> GetCachedFileData(string fileName)
        {
            try
            {
                var file = await GetCachedFile(fileName);
                return new CachedFileData(file);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void WriteEntry(ZipOutputStream zipStream, ZipEntry entry, Stream contentStream)
        {
            zipStream.PutNextEntry(entry);

            var buffer = new byte[4096];
            int sourceBytes = 0;
            do
            {
                sourceBytes = contentStream.Read(buffer, 0, buffer.Length);
                zipStream.Write(buffer, 0, sourceBytes);
            } while (sourceBytes > 0);

            zipStream.CloseEntry();
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

        internal const string FILES_PROTECTION_PASSWORD = "VktTYXZlciAy";
        internal const string FILES_METADATA_NAME = "metadata.vks";
        internal const string FILES_CONTENT_NAME = "content.vks";
        internal const string FILES_EXTENSION = ".vksm";
        internal const string MUSIC_CACHE_FOLDER_NAME = "VKSaver";
    }
}
