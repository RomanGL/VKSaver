using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Interfaces;
using Windows.Storage;
using Windows.Storage.Search;

namespace VKSaver.Core.Services
{
    public sealed class MusicCacheService : IMusicCacheService
    {
        public MusicCacheService(ILogService logService, ILibraryDatabaseService libraryDatabaseService)
        {
            _logService = logService;
            _libraryDatabseService = libraryDatabaseService;
        }

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

                var propertiesToRetrieve = new List<string>();

                propertiesToRetrieve.Add(PROPERTY_SAMPLE_RATE);
                propertiesToRetrieve.Add(PROPERTY_CHANNEL_COUNT);
                propertiesToRetrieve.Add(PROPERTY_ENCODING_BITRATE);

                var encodingProperties = await file.Properties.RetrievePropertiesAsync(propertiesToRetrieve);

                metadata.Track.SampleRate = (uint)encodingProperties[PROPERTY_SAMPLE_RATE];
                metadata.Track.ChannelCount = (uint)encodingProperties[PROPERTY_CHANNEL_COUNT];
                metadata.Track.EncodingBitrate = (uint)encodingProperties[PROPERTY_ENCODING_BITRATE];

                var fileProperties = await file.Properties.GetMusicPropertiesAsync();
                metadata.Track.Duration = fileProperties.Duration.Ticks;

                using (var zipStream = new ZipOutputStream(zipFileStream))
                {
                    zipStream.SetLevel(0);

                    var contentEntry = new ZipEntry(FILES_CONTENT_NAME);
                    contentEntry.CompressionMethod = CompressionMethod.Stored;
                    WriteEntry(zipStream, contentEntry, fileStream);

                    var metadataEntry = new ZipEntry(FILES_METADATA_NAME);
                    metadataEntry.CompressionMethod = CompressionMethod.Stored;
                    WriteEntry(zipStream, metadataEntry, new MemoryStream(
                        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metadata))));

                    zipStream.Finish();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);

                zipFileStream?.Dispose();
                zipFileStream = null;
                await zipFile?.DeleteAsync(StorageDeleteOption.PermanentDelete);
                return false;
            }
            finally
            {
                fileStream?.Dispose();
            }
        }

        public async Task<VKSaverAudioFile> GetVKSaverFile(string fileName)
        {
            try
            {
                var file = await GetCachedFile(fileName);
                return new VKSaverAudioFile(file);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
                return null;
            }
        }

        public async Task<IEnumerable<VKSaverAudioFile>> GetCachedFiles(uint count, uint offset)
        {
            try
            {
                var folder = await GetCacheFolder();
                var files = await folder.GetFilesAsync(CommonFileQuery.DefaultQuery, offset, count);
                
                return files.Select(f => new VKSaverAudioFile(f));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
                return null;
            }
        }

        public async Task<IEnumerable<VKSaverAudioFile>> GetCachedFiles()
        {
            try
            {
                var folder = await GetCacheFolder();
                var files = await folder.GetFilesAsync(CommonFileQuery.DefaultQuery);

                return files.Where(f => f.FileType == FILES_EXTENSION)
                    .Select(f => new VKSaverAudioFile(f));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
                return null;
            }
        }

        public async Task<bool> ClearMusicCache()
        {
            try
            {
                var folder = await GetCacheFolder();
                await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
                return false;
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

        private readonly ILogService _logService;
        private readonly ILibraryDatabaseService _libraryDatabseService;

        internal const string FILES_PROTECTION_PASSWORD = "VktTYXZlciAy";
        internal const string FILES_METADATA_NAME = "metadata.vks";
        internal const string FILES_CONTENT_NAME = "content.vks";
        internal const string FILES_EXTENSION = ".vksm";
        internal const string MUSIC_CACHE_FOLDER_NAME = "VKSaver";

        private const string PROPERTY_SAMPLE_RATE = "System.Audio.SampleRate";
        private const string PROPERTY_CHANNEL_COUNT = "System.Audio.ChannelCount";
        private const string PROPERTY_ENCODING_BITRATE = "System.Audio.EncodingBitrate";        
    }
}
