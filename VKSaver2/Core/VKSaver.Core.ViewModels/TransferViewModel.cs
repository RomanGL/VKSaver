using Microsoft.Practices.Prism.StoreApps;
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
using Windows.UI.Core;

namespace VKSaver.Core.ViewModels
{
    /// <summary>
    /// Представляет модель представления страницы передачи данных.
    /// </summary>
    [ImplementPropertyChanged]
    public class TransferViewModel : ViewModelBase
    {
        public TransferViewModel(IDownloadsService downloadsService, IDialogsService dialogsService,
            IAppLoaderService appLoaderService, ILocService locService, 
            IDispatcherWrapper dispatcherWrapper)
        {
            _downloadsService = downloadsService;
            _dialogsService = dialogsService;
            _appLoaderService = appLoaderService;
            _locService = locService;
            _dispatcherWrapper = dispatcherWrapper;

            Downloads = new ObservableCollection<DownloadItemViewModel>();

            ShowInfoCommand = new DelegateCommand<DownloadItemViewModel>(OnShowInfoCommand);
            CancelDownloadCommand = new DelegateCommand<DownloadItemViewModel>(OnCancelDownloadCommand);
            PauseDownloadCommand = new DelegateCommand<DownloadItemViewModel>(OnPauseResumeDownloadCommand);
            ResumeDownloadCommand = new DelegateCommand<DownloadItemViewModel>(OnPauseResumeDownloadCommand);
            CancelAllDownloadsCommand = new DelegateCommand(OnCancelAllDownloadsCommand, CanExecuteCancelAllDownloadsCommand);
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

        [DoNotNotify]
        public DelegateCommand CancelAllDownloadsCommand { get; private set; }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            _downloadsService.ProgressChanged += OnDownloadProgressChanged;
            _downloadsService.DownloadsCompleted += OnDownloadsCompleted;
            await LoadDownloads();

            UploadsState = ContentState.NoData;
            HistoryState = ContentState.NoData;        

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            _downloadsService.ProgressChanged -= OnDownloadProgressChanged;
            _downloadsService.DownloadsCompleted -= OnDownloadsCompleted;

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }
        
        private async void OnDownloadProgressChanged(object sender, DownloadItem e)
        {
            await _dispatcherWrapper.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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

                CancelAllDownloadsCommand.RaiseCanExecuteChanged();
            });
        }

        private void OnDownloadsCompleted(object sender, EventArgs e)
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
            _downloadsService.Cancel(item.OpeartionGuid);
        }

        private void OnPauseResumeDownloadCommand(DownloadItemViewModel item)
        {
            if (item == null)
                return;
            _downloadsService.PauseResume(item.OpeartionGuid);
        }

        private async void OnCancelAllDownloadsCommand()
        {
            _appLoaderService.Show(_locService["AppLoader_CancelAllDownloads"]);
            await _downloadsService.CancelAll();
        }

        private bool CanExecuteCancelAllDownloadsCommand()
        {
            return Downloads.Count > 0;
        }
        
        private string GetMessageTitleFromStatus(BackgroundTransferStatus status)
        {
            switch (status)
            {
                case BackgroundTransferStatus.Idle:
                    return _locService["Message_Transfer_Idle_Title"];
                case BackgroundTransferStatus.PausedCostedNetwork:
                    return _locService["Message_Transfer_PausedCostedNetwork_Title"];
                case BackgroundTransferStatus.PausedNoNetwork:
                    return _locService["Message_Transfer_PausedNoNetwork_Title"];
                case BackgroundTransferStatus.Error:
                    return _locService["Message_Transfer_Error_Title"];
            }
            return String.Empty;
        }
        
        private string GetMessageTextFromStatus(BackgroundTransferStatus status)
        {
            switch (status)
            {
                case BackgroundTransferStatus.Idle:
                    return _locService["Message_Transfer_Idle_Text"];
                case BackgroundTransferStatus.PausedCostedNetwork:
                    return _locService["Message_Transfer_PausedCostedNetwork_Text"];
                case BackgroundTransferStatus.PausedNoNetwork:
                    return _locService["Message_Transfer_PausedNoNetwork_Text"];
                case BackgroundTransferStatus.Error:
                    return _locService["Message_Transfer_Error_Text"];
            }
            return String.Empty;
        }
        
        private readonly IDownloadsService _downloadsService;
        private readonly IDialogsService _dialogsService;
        private readonly IAppLoaderService _appLoaderService;
        private readonly ILocService _locService;
        private readonly IDispatcherWrapper _dispatcherWrapper;
    }
}
