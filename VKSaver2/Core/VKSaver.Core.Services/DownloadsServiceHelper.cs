using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class DownloadsServiceHelper : IDownloadsServiceHelper
    {
        public DownloadsServiceHelper(IDialogsService dialogsService, 
            IDownloadsService downloadsService)
        {
            _dialogsService = dialogsService;
            _downloadsService = downloadsService;
        }

        public Task<bool> StartDownloadingAsync(IDownloadable item)
        {
            return StartDownloadingAsync(new List<IDownloadable>(1) { item });
        }

        public async Task<bool> StartDownloadingAsync(IList<IDownloadable> items)
        {
            var results = await _downloadsService.StartDownloadingAsync(items);
            if (results.Count > 0)
            {
                ShowError(results);
                return false;
            }

            return true;
        }

        private void ShowError(List<DownloadInitError> errors)
        {
            var sb = new StringBuilder();
            foreach (var error in errors)
            {
                sb.AppendLine($"{error.DownloadItem.FileName} - {GetDownloadInitErrorNameFromType(error.ErrorType)}");
            }

            _dialogsService.Show(sb.ToString(), "Не удалось начать загрузку");
        }

        private static string GetDownloadInitErrorNameFromType(DownloadInitErrorType error)
        {
            switch (error)
            {
                case DownloadInitErrorType.CantCreateFolder:
                    return "Не удалось создать папку";
                case DownloadInitErrorType.CantCreateFile:
                    return "Не удалось создать файл";
                default:
                    return "Неизвестная ошибка";
            }
        }

        private readonly IDialogsService _dialogsService;
        private readonly IDownloadsService _downloadsService;
    }
}
