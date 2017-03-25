using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using Windows.Storage;

namespace VKSaver.Core.Services.Common
{
    internal static class DownloadsExtensions
    {     
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
