using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using ModernDev.InTouch;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Interfaces;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public abstract class VKAudioViewModel : DownloadableAudioViewModel
    {
        protected VKAudioViewModel(
            InTouch inTouch, 
            IAppLoaderService appLoaderService, 
            IDialogsService dialogsService, 
            IInTouchWrapper inTouchWraper, 
            IDownloadsServiceHelper downloadsServiceHelper, 
            IPlayerService playerService,
            ILocService locService, 
            INavigationService navigationService)
            : base(downloadsServiceHelper, appLoaderService, playerService, locService, navigationService)
        {
            _inTouch = inTouch;
            _dialogsService = dialogsService;
            _inTouchWrapper = inTouchWraper;

            AddToMyAudiosCommand = new DelegateCommand<Audio>(OnAddToMyAudiosCommand);
            AddSelectedToMyAudiosCommand = new DelegateCommand(OnAddSelectedToMyAudiosCommand, HasSelectedItems);
        }

        [DoNotNotify]
        public DelegateCommand AddSelectedToMyAudiosCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<Audio> AddToMyAudiosCommand { get; private set; }

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
            SecondaryItems.Add(new AppBarButton
            {
                Label = _locService["AppBarButton_AddToMyAudios_Text"],
                Command = AddSelectedToMyAudiosCommand
            });

            base.CreateSelectionAppBarButtons();
        }

        protected override void OnSelectionChangedCommand()
        {
            AddSelectedToMyAudiosCommand.RaiseCanExecuteChanged();
            base.OnSelectionChangedCommand();
        }

        private async void OnAddToMyAudiosCommand(Audio track)
        {
            _appLoaderService.Show(String.Format(_locService["AppLoader_AddingItem"], track.ToString()));

            bool isSuccess = false;
            try
            {
                isSuccess = await AddToMyAudios(track);
            }
            catch (Exception) { }

            if (!isSuccess)
            {
                _dialogsService.Show(_locService["Message_AudioAddError_Text"],
                    _locService["Message_AudioAddError_Title"]);
            }
            _appLoaderService.Hide();
        }

        private async void OnAddSelectedToMyAudiosCommand()
        {
            _appLoaderService.Show(_locService["AppLoader_Preparing"]);
            _cancelOperations = false;

            var items = SelectedItems.Cast<Audio>().ToList();
            var errors = new List<Audio>();

            foreach (var track in items)
            {
                if (_cancelOperations)
                {
                    errors.Add(track);
                    continue;
                }

                _appLoaderService.Show(String.Format(_locService["AppLoader_AddingItem"], track.ToString()));

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

        private async Task<bool> AddToMyAudios(Audio audio)
        {
            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.Add(
                audio.Id, audio.OwnerId));

            if (response.IsCaptchaError())
                throw new Exception("Captcha error: cancel");
            return !response.IsError;
        }

        private void ShowAddingError(List<Audio> errorTracks)
        {
            var sb = new StringBuilder();
            sb.AppendLine(_locService["Message_AddSelectedError_Text"]);
            sb.AppendLine();

            foreach (var track in errorTracks)
            {
                sb.AppendLine(track.ToString());
            }

            _dialogsService.Show(sb.ToString(), _locService["Message_AddSelectedError_Title"]);
        }

        private bool _cancelOperations = false;

        protected readonly IDialogsService _dialogsService;
        protected readonly InTouch _inTouch;
        protected readonly IInTouchWrapper _inTouchWrapper;
    }
}
