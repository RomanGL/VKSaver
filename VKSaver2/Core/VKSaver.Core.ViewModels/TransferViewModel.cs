﻿using Microsoft.Practices.Prism.StoreApps;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Services.Transfer;
using VKSaver.Core.Transfer;
using VKSaver.Core.ViewModels.Transfer;
using Windows.Networking.BackgroundTransfer;

namespace VKSaver.Core.ViewModels
{
    /// <summary>
    /// Представляет модель представления страницы передачи данных.
    /// </summary>
    [ImplementPropertyChanged]
    public class TransferViewModel : ViewModelBase
    {
        public TransferViewModel(IDownloadsService downloadsService, IDialogsService dialogsService)
        {
            _downloadsService = downloadsService;
            _dialogsService = dialogsService;

            Downloads = new ObservableCollection<DownloadItemViewModel>();

            ShowInfoCommand = new DelegateCommand<DownloadItemViewModel>(OnShowInfoCommand);
            CancelDownloadCommand = new DelegateCommand<DownloadItemViewModel>(OnCancelDownloadCommand);
            PauseDownloadCommand = new DelegateCommand<DownloadItemViewModel>(OnPauseResumeDownloadCommand);
            ResumeDownloadCommand = new DelegateCommand<DownloadItemViewModel>(OnPauseResumeDownloadCommand);
        }
        
        [DoNotNotify]
        public ObservableCollection<DownloadItemViewModel> Downloads { get; private set; }

        public ContentState DownloadsState { get; private set; }
        public ContentState UploadsState { get; private set; }
        public ContentState HistoryState { get; private set; }

        [DoNotNotify]
        public DelegateCommand<DownloadItemViewModel> ShowInfoCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<DownloadItemViewModel> PauseDownloadCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<DownloadItemViewModel> ResumeDownloadCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<DownloadItemViewModel> CancelDownloadCommand { get; private set; }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            _downloadsService.ProgressChanged += OnDownloadProgressChanged;
            await LoadDownloads();

            UploadsState = ContentState.NoData;
            HistoryState = ContentState.NoData;        

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            _downloadsService.ProgressChanged -= OnDownloadProgressChanged;

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }
        
        private void OnDownloadProgressChanged(object sender, DownloadItem e)
        {
            if (DownloadsState == ContentState.Loading)
                return;

            DownloadsState = ContentState.Normal;
            var item = Downloads.FirstOrDefault(d => d.OpeartionGuid == e.OperationGuid);
            if (item == null)
            {
                if (e.Status != BackgroundTransferStatus.Completed &&
                    e.Status != BackgroundTransferStatus.Canceled)
                    return;

                Downloads.Add(new DownloadItemViewModel(e));
                item = Downloads.FirstOrDefault(d => d.OpeartionGuid == e.OperationGuid);
                if (item == null)
                    return;
            }
            else
                item.Download = e;

            if (item.Status == BackgroundTransferStatus.Completed ||
                item.Status == BackgroundTransferStatus.Canceled)
                Downloads.Remove(item);

            if (Downloads.Count == 0)
                DownloadsState = ContentState.NoData;
            else
                Downloads.OrderBy(d => d.Status, new TransferStatusComparer());
        }
        
        private async Task LoadDownloads()
        {
            Downloads.Clear();
            DownloadsState = ContentState.Loading;

            while (_downloadsService.IsLoading)
                await Task.Delay(1000);

            var downloads = _downloadsService.GetAllDownloads();
            if (downloads == null)
            {
                DownloadsState = ContentState.NoData;
                return;
            }

            for (int i = 0; i < downloads.Length; i++)
                Downloads.Add(new DownloadItemViewModel(downloads[i]));

            DownloadsState = ContentState.Normal;
        }

        private void OnShowInfoCommand(DownloadItemViewModel item)
        {
            if (item.Status == BackgroundTransferStatus.Error ||
                item.Status == BackgroundTransferStatus.Idle ||
                item.Status == BackgroundTransferStatus.PausedCostedNetwork ||
                item.Status == BackgroundTransferStatus.PausedNoNetwork)
            {
                _dialogsService.Show(GetMessageTextFromStatus(item.Status),
                    GetMessageTitleFromStatus(item.Status));
            }
        }

        private void OnCancelDownloadCommand(DownloadItemViewModel item)
        {
            if (item == null)
                return;
            _downloadsService.CancelDownload(item.OpeartionGuid);
        }

        private void OnPauseResumeDownloadCommand(DownloadItemViewModel item)
        {
            if (item == null)
                return;
            _downloadsService.PauseResume(item.OpeartionGuid);
        }
        
        private static string GetMessageTitleFromStatus(BackgroundTransferStatus status)
        {
            switch (status)
            {
                case BackgroundTransferStatus.Idle:
                    return "Ожидание очереди";
                case BackgroundTransferStatus.PausedCostedNetwork:
                    return "Платная сеть";
                case BackgroundTransferStatus.PausedNoNetwork:
                    return "Ожидание соединения";
                case BackgroundTransferStatus.Error:
                    return "Произошла ошибка";
            }
            return String.Empty;
        }
        
        private static string GetMessageTextFromStatus(BackgroundTransferStatus status)
        {
            switch (status)
            {
                case BackgroundTransferStatus.Idle:
                    return "Ожидание очереди загрузки.";
                case BackgroundTransferStatus.PausedCostedNetwork:
                    return "Windows заблокировала загрузку, так как за использование соединения может взиматься плата. Попробуйте убрать все ограничения в \"Контроле данных\" и отключите лимит трафика.";
                case BackgroundTransferStatus.PausedNoNetwork:
                    return "Нет доступа к сети. Проверьте настройки передачи данных и повторите попытку.";
                case BackgroundTransferStatus.Error:
                    return "К сожалению, при загрузке произошла ошибка и мы не можем ее закончить.";
            }
            return String.Empty;
        }
        
        private readonly IDownloadsService _downloadsService;
        private readonly IDialogsService _dialogsService;
    }
}
