﻿#if WINDOWS_UWP
using Prism.Commands;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
#elif ANDROID
using VKSaver.Core.Toolkit.Commands;
#endif

using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Transfer;
using VKSaver.Core.Toolkit;
using NavigatedToEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatedToEventArgs;
using NavigatingFromEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatingFromEventArgs;

namespace VKSaver.Core.ViewModels
{
    /// <summary>
    /// Представляет модель представления страницы передачи данных.
    /// </summary>
    [ImplementPropertyChanged]
    public class TransferViewModel : VKSaverViewModel
    {
        public TransferViewModel(
            IDownloadsService downloadsService, 
            IUploadsService uploadsService,
            IDialogsService dialogsService,
            IAppLoaderService appLoaderService, 
            ILocService locService, 
            IDispatcherWrapper dispatcherWrapper)
        {
            _downloadsService = downloadsService;
            _uploadsService = uploadsService;
            _dialogsService = dialogsService;
            _appLoaderService = appLoaderService;
            _locService = locService;
            _dispatcherWrapper = dispatcherWrapper;

            Downloads = new ObservableCollection<TransferItemViewModel>();
            Uploads = new ObservableCollection<TransferItemViewModel>();

            ShowInfoCommand = new DelegateCommand<TransferItemViewModel>(OnShowInfoCommand);
            CancelDownloadCommand = new DelegateCommand<TransferItemViewModel>(OnCancelDownloadCommand);
            CancelUploadCommand = new DelegateCommand<TransferItemViewModel>(OnCancelUploadCommand);
            PauseDownloadCommand = new DelegateCommand<TransferItemViewModel>(OnPauseResumeDownloadCommand);
            ResumeDownloadCommand = new DelegateCommand<TransferItemViewModel>(OnPauseResumeDownloadCommand);
            CancelAllDownloadsCommand = new DelegateCommand(OnCancelAllDownloadsCommand, CanExecuteCancelAllDownloadsCommand);
            CancelAllUploadsCommand = new DelegateCommand(OnCancelAllUploadsCommand, CanExecuteCancelAllUploadsCommand);
        }
        
        [DoNotNotify]
        public ObservableCollection<TransferItemViewModel> Downloads { get; private set; }

        [DoNotNotify]
        public ObservableCollection<TransferItemViewModel> Uploads { get; private set; }

        public int SelectedPivotIndex { get; set; }

        public ContentState DownloadsState { get; private set; }
        public ContentState UploadsState { get; private set; }
        public ContentState HistoryState { get; private set; }

        [DoNotNotify]
        public DelegateCommand<TransferItemViewModel> ShowInfoCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<TransferItemViewModel> PauseDownloadCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<TransferItemViewModel> ResumeDownloadCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<TransferItemViewModel> CancelDownloadCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<TransferItemViewModel> CancelUploadCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand CancelAllDownloadsCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand CancelAllUploadsCommand { get; private set; }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (e.Parameter != null)
            {
                string view = e.Parameter.ToString();
                switch (view)
                {
                    case "downloads":
                        SelectedPivotIndex = 0;
                        break;
                    case "uploads":
                        SelectedPivotIndex = 1;
                        break;
                }
            }

            _downloadsService.ProgressChanged += OnDownloadProgressChanged;
            _downloadsService.DownloadsCompleted += OnDownloadsUploadsCompleted;

            _uploadsService.ProgressChanged += OnUploadProgressChanged;
            _uploadsService.UploadsCompleted += OnDownloadsUploadsCompleted;

            await LoadDownloads();
            await LoadUploads();
            
            HistoryState = ContentState.NoData;        

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (!suspending)
            {
                _downloadsService.ProgressChanged -= OnDownloadProgressChanged;
                _downloadsService.DownloadsCompleted -= OnDownloadsUploadsCompleted;

                _uploadsService.ProgressChanged -= OnUploadProgressChanged;
                _uploadsService.UploadsCompleted -= OnDownloadsUploadsCompleted;
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }
        
        private async void OnDownloadProgressChanged(object sender, TransferItem e)
        {
            await _dispatcherWrapper.RunOnUIThreadAsync(() =>
            {
                if (DownloadsState == ContentState.Loading)
                    return;

                DownloadsState = ContentState.Normal;
                var item = Downloads.FirstOrDefault(d => d.OperationGuid == e.OperationGuid);
                if (item == null)
                {
                    if (e.Status != VKSaverTransferStatus.Completed &&
                        e.Status != VKSaverTransferStatus.Canceled)
                        return;

                    Downloads.Add(new TransferItemViewModel(e, _locService));
                    item = Downloads.FirstOrDefault(d => d.OperationGuid == e.OperationGuid);
                    if (item == null)
                        return;
                }
                else
                    item.Operation = e;

                if (item.Status == VKSaverTransferStatus.Completed ||
                    item.Status == VKSaverTransferStatus.Canceled)
                    Downloads.Remove(item);

                if (Downloads.Count == 0)
                    DownloadsState = ContentState.NoData;
                else
                    Downloads.OrderBy(d => d.Status, new TransferStatusComparer());

                CancelAllDownloadsCommand.RaiseCanExecuteChanged();
            });
        }

        private async void OnUploadProgressChanged(object sender, TransferItem e)
        {
            await _dispatcherWrapper.RunOnUIThreadAsync(() =>
            {
                if (UploadsState == ContentState.Loading)
                    return;

                UploadsState = ContentState.Normal;
                var item = Uploads.FirstOrDefault(d => d.OperationGuid == e.OperationGuid);
                if (item == null)
                {
                    if (e.Status != VKSaverTransferStatus.Completed &&
                        e.Status != VKSaverTransferStatus.Canceled)
                        return;

                    Uploads.Add(new TransferItemViewModel(e, _locService));
                    item = Uploads.FirstOrDefault(d => d.OperationGuid == e.OperationGuid);
                    if (item == null)
                        return;
                }
                else
                    item.Operation = e;

                if (item.Status == VKSaverTransferStatus.Completed ||
                    item.Status == VKSaverTransferStatus.Canceled)
                    Uploads.Remove(item);

                if (Uploads.Count == 0)
                    UploadsState = ContentState.NoData;
                else
                    Uploads.OrderBy(d => d.Status, new TransferStatusComparer());
            });
        }

        private void OnDownloadsUploadsCompleted(object sender, EventArgs e)
        {
            _appLoaderService.Hide();
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
                Downloads.Add(new TransferItemViewModel(downloads[i], _locService));

            DownloadsState = ContentState.Normal;
        }

        private async Task LoadUploads()
        {
            Uploads.Clear();
            UploadsState = ContentState.Loading;

            while (_uploadsService.IsLoading)
                await Task.Delay(1000);

            var uploads = _uploadsService.GetAllUploads();
            if (uploads == null)
            {
                UploadsState = ContentState.NoData;
                return;
            }

            foreach (var upload in uploads)
            {
                Uploads.Add(new TransferItemViewModel(upload, _locService));
            }

            UploadsState = ContentState.Normal;
        }

        private void OnShowInfoCommand(TransferItemViewModel item)
        {
            if (item.Status == VKSaverTransferStatus.Error ||
                item.Status == VKSaverTransferStatus.Idle ||
                item.Status == VKSaverTransferStatus.PausedCostedNetwork ||
                item.Status == VKSaverTransferStatus.PausedNoNetwork)
            {
                _dialogsService.Show(GetMessageTextFromStatus(item.Status),
                    GetMessageTitleFromStatus(item.Status));
            }
        }

        private void OnCancelDownloadCommand(TransferItemViewModel item)
        {
            if (item == null)
                return;
            _downloadsService.Cancel(item.OperationGuid);
        }

        private void OnCancelUploadCommand(TransferItemViewModel item)
        {
            if (item == null)
                return;
            _uploadsService.Cancel(item.OperationGuid);
        }

        private void OnPauseResumeDownloadCommand(TransferItemViewModel item)
        {
            if (item == null)
                return;
            _downloadsService.PauseResume(item.OperationGuid);
        }

        private async void OnCancelAllDownloadsCommand()
        {
            _appLoaderService.Show(_locService["AppLoader_CancelAllDownloads"]);
            await _downloadsService.CancelAllAsync();
        }

        private async void OnCancelAllUploadsCommand()
        {
            _appLoaderService.Show(_locService["AppLoader_CancelAllUploads"]);
            await _uploadsService.CancelAllAsync();
        }

        private bool CanExecuteCancelAllDownloadsCommand()
        {
            return Downloads.Count > 0;
        }

        private bool CanExecuteCancelAllUploadsCommand()
        {
            return Uploads.Count > 0;
        }
        
        private string GetMessageTitleFromStatus(VKSaverTransferStatus status)
        {
            switch (status)
            {
                case VKSaverTransferStatus.Idle:
                    return _locService["Message_Transfer_Idle_Title"];
                case VKSaverTransferStatus.PausedCostedNetwork:
                    return _locService["Message_Transfer_PausedCostedNetwork_Title"];
                case VKSaverTransferStatus.PausedNoNetwork:
                    return _locService["Message_Transfer_PausedNoNetwork_Title"];
                case VKSaverTransferStatus.Error:
                    return _locService["Message_Transfer_Error_Title"];
            }
            return String.Empty;
        }
        
        private string GetMessageTextFromStatus(VKSaverTransferStatus status)
        {
            switch (status)
            {
                case VKSaverTransferStatus.Idle:
                    return _locService["Message_Transfer_Idle_Text"];
                case VKSaverTransferStatus.PausedCostedNetwork:
                    return _locService["Message_Transfer_PausedCostedNetwork_Text"];
                case VKSaverTransferStatus.PausedNoNetwork:
                    return _locService["Message_Transfer_PausedNoNetwork_Text"];
                case VKSaverTransferStatus.Error:
                    return _locService["Message_Transfer_Error_Text"];
            }
            return String.Empty;
        }
        
        private readonly IDownloadsService _downloadsService;
        private readonly IUploadsService _uploadsService;
        private readonly IDialogsService _dialogsService;
        private readonly IAppLoaderService _appLoaderService;
        private readonly ILocService _locService;
        private readonly IDispatcherWrapper _dispatcherWrapper;        
    }
}
