﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class DownloadsServiceHelper : IDownloadsServiceHelper
    {
        public DownloadsServiceHelper(
            IDialogsService dialogsService, 
            IDownloadsService downloadsService, 
            ILocService locService,
            IAppNotificationsService appNotificationsService)
        {
            _dialogsService = dialogsService;
            _downloadsService = downloadsService;
            _locService = locService;
            _appNotificationsService = appNotificationsService;
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
                if (results.Count == items.Count)
                {
                    ShowFailNotification(results);
                }
                else
                {
                    ShowFailNotification(results);
                    _appNotificationsService.SendNotification(new AppNotification
                    {
                        Title = "Загрузка началась, но не все элементы будут загружены",
                        Content = "Коснитесь для перехода в менеджер загрузок",
                        Type = AppNotificationType.Warning
                    });
                }

                return false;
            }
            else
            {
                ShowSuccessNotification();
                return true;
            }
        }

        private void ShowSuccessNotification()
        {
            _appNotificationsService.SendNotification(new AppNotification
            {
                Title = "Загрузка началась",
                Content = "Коснитесь для перехода в менеджер загрузок",
                Type = AppNotificationType.Info
            });
        }

        private void ShowFailNotification(List<DownloadInitError> errors)
        {
            _appNotificationsService.SendNotification(new AppNotification
            {
                Title = "Не удалось начать загрузку",
                Content = "Коснитесь для подробностей",
                Type = AppNotificationType.Error,
                ActionToDo = () => ShowError(errors)
            });
        }

        private void ShowError(List<DownloadInitError> errors)
        {
            var groups = errors.GroupBy(e => e.ErrorType);
            var sb = new StringBuilder();
            foreach (var group in groups)
            {
                sb.AppendLine(GetDownloadInitErrorNameFromType(group.Key));
                foreach (var error in group)
                {
                    if (error.DownloadItem.ContentType == FileContentType.Music)
                        sb.AppendLine(((VKSaverAudio)error.DownloadItem.Metadata).Track.Title);
                    else
                        sb.AppendLine(error.DownloadItem.FileName);
                }

                sb.AppendLine();
            }

            _dialogsService.Show(sb.ToString(), _locService["Message_Downloads_StartFailed_Title"]);
        }

        private string GetDownloadInitErrorNameFromType(DownloadInitErrorType error)
        {
            switch (error)
            {
                case DownloadInitErrorType.CantCreateFolder:
                    return _locService["Message_Downloads_CantCreateFolder_Text"];
                case DownloadInitErrorType.CantCreateFile:
                    return _locService["Message_Downloads_CantCreateFile_Text"];
                case DownloadInitErrorType.MaxDownloadsExceeded:
                    return _locService["Message_Downloads_MaxDownloadsExceeded_Text"];
                default:
                    return _locService["Message_Downloads_UnknownError_Text"];
            }
        }

        private readonly IDialogsService _dialogsService;
        private readonly IDownloadsService _downloadsService;
        private readonly ILocService _locService;
        private readonly IAppNotificationsService _appNotificationsService;
    }
}
