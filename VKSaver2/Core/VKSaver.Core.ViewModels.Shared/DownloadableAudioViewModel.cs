#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#endif

using PropertyChanged;
using System.Linq;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services.Interfaces;
using Windows.UI.Xaml.Controls;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public abstract class DownloadableAudioViewModel<T> : AudioViewModel<T>
    {
        protected DownloadableAudioViewModel(
            IDownloadsServiceHelper downloadsServiceHelper,
            IAppLoaderService appLoaderService, 
            IPlayerService playerService, 
            ILocService locService,
            INavigationService navigationService,
            int maxPlayingTracks = -1)
            : base(playerService, locService, navigationService, appLoaderService, maxPlayingTracks)
        {
            _downloadsServiceHelper = downloadsServiceHelper;            

            DownloadTrackCommand = new DelegateCommand<T>(OnDownloadTrackCommand, CanDownloadTrack);
            DownloadSelectedCommand = new DelegateCommand(OnDownloadSelectedCommand, () => HasSelectedItems() & CanDownloadSelected());
            OpenTransferManagerCommand = new DelegateCommand(OnOpenTransferManagerCommand);
        }

        [DoNotNotify]
        public DelegateCommand OpenTransferManagerCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<T> DownloadTrackCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand DownloadSelectedCommand { get; private set; }

        protected virtual bool CanDownloadTrack(T track) => true;
        protected virtual bool CanDownloadSelected() => true;
        protected abstract IDownloadable ConvertToDownloadable(T track);

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

        private async void OnDownloadTrackCommand(T track)
        {
            await _downloadsServiceHelper.StartDownloadingAsync(ConvertToDownloadable(track));
        }

        private async void OnDownloadSelectedCommand()
        {
            _appLoaderService.Show(_locService["AppLoader_PreparingFilesToDownload"]);
            var items = SelectedItems.ToList();
            SetDefaultMode();

            var toDownload = items.Cast<T>().Where(CanDownloadTrack).Select(ConvertToDownloadable).ToList();
            await _downloadsServiceHelper.StartDownloadingAsync(toDownload);
            _appLoaderService.Hide();
        }

        protected readonly IDownloadsServiceHelper _downloadsServiceHelper;        
    }
}
