using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public const string CAPATIBLE_MUSIC_FOLDER_NAME = "RootMusic";
        private const string STORAGE_MUSIC_FOLDER = @"\Music\";
    }
}
