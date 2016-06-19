using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace VKSaver.Core.Services.Common
{
    internal static class DownloadsExtensions
    {        
        internal static FileContentType GetContentTypeFromExtension(string fileExtension)
        {
            switch (fileExtension)
            {
                case ".mp3":
                case ".wma":
                case ".wav":
                case ".aac":
                case ".m4a":
                case ".flac":
                    return FileContentType.Music;

                case ".mp4":
                case ".mkv":
                case ".avi":
                case ".3gp":
                    return FileContentType.Video;

                case ".jpg":
                case ".jpe":
                case ".jpeg":
                case ".bmp":
                case ".png":
                case ".gif":
                    return FileContentType.Image;

                default: return FileContentType.Other;
            }
        }

        internal static async Task<StorageFolder> GetFolderFromType(FileContentType type)
        {
            try
            {
                StorageFolder rootFolder = null;

                switch (type)
                {
                    case FileContentType.Music:
                        rootFolder = KnownFolders.MusicLibrary;
                        break;
                    case FileContentType.Video:
                        rootFolder = KnownFolders.VideosLibrary;
                        break;
                    case FileContentType.Image:
                        rootFolder = KnownFolders.SavedPictures;
                        break;
                    default:
                        return await KnownFolders.PicturesLibrary.CreateFolderAsync(DOWNLOADS_OTHER_FOLDER_NAME,
                            CreationCollisionOption.OpenIfExists);
                }

                return await rootFolder.CreateFolderAsync(DOWNLOADS_FOLDER_NAME,
                    CreationCollisionOption.OpenIfExists);
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal static string GetSafeFileName(string fileName)
        {
            if (fileName.Length > 50)
                fileName = fileName.Remove(50);
            
            return _safeRegex.Replace(fileName, String.Empty);
        }

        private static readonly Regex _safeRegex = new Regex("[?*.<>:|&/\"]");

        private const string DOWNLOADS_FOLDER_NAME = "VKSaver";
        private const string DOWNLOADS_OTHER_FOLDER_NAME = "VKSaver Other";
    }
}
