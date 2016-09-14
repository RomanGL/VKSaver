using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using ModernDev.InTouch;
using PropertyChanged;
using System.Collections.Generic;
using System.Linq;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Common;
using Windows.UI.Xaml.Controls;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public abstract class DownloadableAudioViewModel : AudioViewModel
    {
        protected DownloadableAudioViewModel(
            IDownloadsServiceHelper downloadsServiceHelper,
            IAppLoaderService appLoaderService, 
            IPlayerService playerService, 
            ILocService locService,
            INavigationService navigationService)
            : base(playerService, locService, navigationService, appLoaderService)
        {
            _downloadsServiceHelper = downloadsServiceHelper;            

            DownloadTrackCommand = new DelegateCommand<Audio>(OnDownloadTrackCommand);
            DownloadSelectedCommand = new DelegateCommand(OnDownloadSelectedCommand, HasSelectedItems);
            OpenTransferManagerCommand = new DelegateCommand(OnOpenTransferManagerCommand);
        }

        [DoNotNotify]
        public DelegateCommand OpenTransferManagerCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<Audio> DownloadTrackCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand DownloadSelectedCommand { get; private set; }

        protected override void OnSelectionChangedCommand()
        {
            DownloadSelectedCommand.RaiseCanExecuteChanged();
            base.OnSelectionChangedCommand();
        }

        protected override void CreateDefaultAppBarButtons()
        {
            SecondaryItems.Add(new AppBarButton
            {
                Label = _locService["AppBarButton_TransferManager_Text"],
                Command = OpenTransferManagerCommand
            });

            base.CreateDefaultAppBarButtons();
        }

        protected override void CreateSelectionAppBarButtons()
        {
            AppBarItems.Add(new AppBarButton
            {
                Label = _locService["AppBarButton_Download_Text"],
                Icon = new FontIcon { Glyph = "\uE118" },
                Command = DownloadSelectedCommand
            });

            base.CreateSelectionAppBarButtons();
        }

        private void OnOpenTransferManagerCommand()
        {
            _navigationService.Navigate("TransferView", "downloads");
        }

        private async void OnDownloadTrackCommand(Audio track)
        {
            await _downloadsServiceHelper.StartDownloadingAsync(track.ToDownloadable());
        }

        private async void OnDownloadSelectedCommand()
        {
            _appLoaderService.Show(_locService["AppLoader_PreparingFilesToDownload"]);
            var items = SelectedItems.ToList();
            SetDefaultMode();

            var toDownload = new List<IDownloadable>(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                toDownload.Add(((Audio)items[i]).ToDownloadable());
            }

            await _downloadsServiceHelper.StartDownloadingAsync(toDownload);
            _appLoaderService.Hide();
        }

        protected readonly IDownloadsServiceHelper _downloadsServiceHelper;        
    }
}
