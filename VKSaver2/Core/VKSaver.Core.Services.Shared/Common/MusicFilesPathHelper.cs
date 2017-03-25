using System;
using System.Threading.Tasks;
using VKSaver.Core.FileSystem;

#if !ANDROID
using Windows.Storage;
using Windows.Storage.AccessCache;
#endif

namespace VKSaver.Core.Services.Common
{
    public static class MusicFilesPathHelper
    {
        /// <summary>
        /// Возвращает поддерживаемый приложением путь к локальному аудио файлу.
        /// </summary>
        public static string GetCapatibleSource(string originalSource)
        {
#if WINDOWS_UWP
            return originalSource;
#else
            int index = originalSource.IndexOf(STORAGE_MUSIC_FOLDER);
            string newPath = originalSource.Remove(0, index + 6);

            return CAPATIBLE_MUSIC_FOLDER_NAME + newPath;
#endif
        }

        public static async Task<IFile> GetFileFromCapatibleName(string path)
        {
#if ANDROID
            throw new NotImplementedException("IFile");
#else
            try
            {
                if (path.StartsWith("http"))
                    return null;
                else if (path.StartsWith("vks-token:"))
                    return new File(await StorageApplicationPermissions.FutureAccessList.GetFileAsync(path.Substring(10)));

#if WINDOWS_UWP
                return new File(await StorageFile.GetFileFromPathAsync(path));
#else
                var blocks = path.Split(new char[] { '\\' });

                IFolder currentFolder = null;
                IFile file = null;

                for (int i = 0; i < blocks.Length; i++)
                {
                    if (i == 0 && blocks[0] == CAPATIBLE_MUSIC_FOLDER_NAME)
                        currentFolder = new Folder(KnownFolders.MusicLibrary);
                    else if (i == blocks.Length - 1)
                    {
                        file = await currentFolder.GetFileAsync(blocks[i]);
                        break;
                    }
                    else
                        currentFolder = await currentFolder.GetFolderAsync(blocks[i]);
                }

                return file;
#endif
            }
            catch (Exception)
            {
                return null;
            }
#endif
        }

        public const string CAPATIBLE_MUSIC_FOLDER_NAME = "RootMusic";
        private const string STORAGE_MUSIC_FOLDER = @"\Music\";
    }
}
