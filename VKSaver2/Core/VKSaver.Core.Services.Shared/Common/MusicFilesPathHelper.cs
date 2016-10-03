using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace VKSaver.Core.Services.Common
{
    public static class MusicFilesPathHelper
    {
        /// <summary>
        /// Возвращает поддерживаемый приложением путь к локальному аудио файлу.
        /// </summary>
        public static string GetCapatibleSource(string originalSource)
        {
            int index = originalSource.IndexOf(STORAGE_MUSIC_FOLDER);
            string newPath = originalSource.Remove(0, index + 6);

            return CAPATIBLE_MUSIC_FOLDER_NAME + newPath;
        }

        public static async Task<StorageFile> GetFileFromCapatibleName(string path)
        {
            try
            {
                if (path.StartsWith("http"))
                    return null;
                else if (path.StartsWith("vks-token:"))
                    return await StorageApplicationPermissions.FutureAccessList.GetFileAsync(path.Substring(10));

                var blocks = path.Split(new char[] { '\\' });

                StorageFolder currentFolder = null;
                StorageFile file = null;

                for (int i = 0; i < blocks.Length; i++)
                {
                    if (i == 0 && blocks[0] == CAPATIBLE_MUSIC_FOLDER_NAME)
                        currentFolder = KnownFolders.MusicLibrary;
                    else if (i == blocks.Length - 1)
                    {
                        file = await currentFolder.GetFileAsync(blocks[i]);
                        break;
                    }
                    else
                        currentFolder = await currentFolder.GetFolderAsync(blocks[i]);
                }

                return file;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public const string CAPATIBLE_MUSIC_FOLDER_NAME = "RootMusic";
        private const string STORAGE_MUSIC_FOLDER = @"\Music\";
    }
}
