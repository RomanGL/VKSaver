using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using ModernDev.InTouch;
using Newtonsoft.Json;
using PropertyChanged;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public abstract class VKAudioViewModel<T> : DownloadableAudioViewModel<T>
    {
        protected VKAudioViewModel(
            InTouch inTouch,
            IAppLoaderService appLoaderService,
            IDialogsService dialogsService,
            IInTouchWrapper inTouchWrapper,
            IDownloadsServiceHelper downloadsServiceHelper,
            IPlayerService playerService,
            ILocService locService,
            INavigationService navigationService,
            IPurchaseService purchaseService)
            : base(downloadsServiceHelper, appLoaderService, playerService, locService, navigationService)
        {
            _inTouch = inTouch;
            _dialogsService = dialogsService;
            _inTouchWrapper = inTouchWrapper;
            _purchaseService = purchaseService;

            AddToMyAudiosCommand = new DelegateCommand<T>(OnAddToMyAudiosCommand, CanAddToMyAudios);
            AddSelectedToMyAudiosCommand = new DelegateCommand(OnAddSelectedToMyAudiosCommand, () => HasSelectedItems() & CanAddSelectedAudios());
            ShowTrackInfoCommand = new DelegateCommand<T>(OnShowTrackInfoCommand, CanShowTrackInfo);
        }

        [DoNotNotify]
        public DelegateCommand AddSelectedToMyAudiosCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<T> AddToMyAudiosCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<T> ShowTrackInfoCommand { get; private set; }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.Back && _appLoaderService.IsShowed)
            {
                e.Cancel = true;
                _cancelOperations = true;
                _appLoaderService.Hide();
                return;
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        protected override void CreateSelectionAppBarButtons()
        {
            if (AddSelectedToMyAudiosSupported())
            {
                SecondaryItems.Add(new AppBarButton
                {
                    Label = _locService["AppBarButton_AddToMyAudios_Text"],
                    Command = AddSelectedToMyAudiosCommand
                });
            }

            base.CreateSelectionAppBarButtons();
        }

        protected override void OnSelectionChangedCommand()
        {
            AddSelectedToMyAudiosCommand.RaiseCanExecuteChanged();
            base.OnSelectionChangedCommand();
        }

        protected abstract VKAudioInfo GetAudioInfo(T track);

        protected virtual bool CanShowTrackInfo(T audio) => true;
        protected virtual bool CanAddToMyAudios(T audio) => true;
        protected virtual bool CanAddSelectedAudios() => true;
        protected virtual bool AddSelectedToMyAudiosSupported() => true;
        protected virtual string TrackToFriendlyNameString(T track) => track.ToString();

        protected virtual void OnAddAudioSuccess(T track)
        {
        }

        protected virtual void OnAddAudioFail(T track)
        {
            _dialogsService.Show(_locService["Message_AudioAddError_Text"],
                    _locService["Message_AudioAddError_Title"]);
        }

        protected virtual void OnAddAudioStarted(T track)
        {
            _appLoaderService.Show(String.Format(_locService["AppLoader_AddingItem"], TrackToFriendlyNameString(track)));
        }

        protected virtual void OnAddAudioFinished(T track)
        {
            _appLoaderService.Hide();
        }

        private void OnShowTrackInfoCommand(T track)
        {
            var info = GetAudioInfo(track);
            string parameter = JsonConvert.SerializeObject(info);

            if (_purchaseService.IsFullVersionPurchased)
                _navigationService.Navigate("VKAudioInfoView", parameter);
            else
                _navigationService.Navigate("PurchaseView", JsonConvert.SerializeObject(
                    new KeyValuePair<string, string>("VKAudioInfoView", parameter)));
        }

        private async void OnAddToMyAudiosCommand(T track)
        {
            OnAddAudioStarted(track);

            bool isSuccess = false;
            try
            {
                isSuccess = await AddToMyAudios(track);
            }
            catch (Exception) { }

            if (isSuccess)
                OnAddAudioSuccess(track);
            else
                OnAddAudioFail(track);

            OnAddAudioFinished(track);
        }

        private async void OnAddSelectedToMyAudiosCommand()
        {
            _appLoaderService.Show(_locService["AppLoader_Preparing"]);
            _cancelOperations = false;

            var items = SelectedItems.Cast<T>().ToList();
            var errors = new List<T>();

            foreach (var track in items)
            {
                if (_cancelOperations)
                {
                    errors.Add(track);
                    continue;
                }

                _appLoaderService.Show(String.Format(_locService["AppLoader_AddingItem"], TrackToFriendlyNameString(track)));

                bool isSuccess = false;
                try
                {
                    isSuccess = await AddToMyAudios(track);
                }
                catch (Exception)
                {
                    errors.Add(track);
                    _cancelOperations = true;
                    _appLoaderService.Show(_locService["AppLoader_PleaseWait"]);
                    continue;
                }

                if (!isSuccess)
                    errors.Add(track);

                await Task.Delay(200);
            }

            if (errors.Any())
                ShowAddingError(errors);

            _appLoaderService.Hide();
        }

        private async Task<bool> AddToMyAudios(T audio)
        {
            var info = GetAudioInfo(audio);
            if (info == null)
                return false;

            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.Add(
                info.Id, info.OwnerId));

            if (response.IsCaptchaError())
                throw new Exception("Captcha error: cancel");
            return !response.IsError;
        }

        private void ShowAddingError(List<T> errorTracks)
        {
            var sb = new StringBuilder();
            sb.AppendLine(_locService["Message_AddSelectedError_Text"]);
            sb.AppendLine();

            foreach (var track in errorTracks)
            {
                sb.AppendLine(TrackToFriendlyNameString(track));
            }

            _dialogsService.Show(sb.ToString(), _locService["Message_AddSelectedError_Title"]);
        }

        protected bool _cancelOperations = false;

        protected readonly IDialogsService _dialogsService;
        protected readonly InTouch _inTouch;
        protected readonly IInTouchWrapper _inTouchWrapper;
        protected readonly IPurchaseService _purchaseService;
    }
}
