#if WINDOWS_UWP
using Prism.Commands;
using Prism.Windows.Navigation;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#elif ANDROID
using VkSaver.Core.Toolkit.Commands;
#endif

using ModernDev.InTouch;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Services.Common;
using VKSaver.Core.ViewModels.Common;
using Newtonsoft.Json;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Toolkit.Controls;
using VKSaver.Core.Toolkit.Navigation;
using NavigatingFromEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatingFromEventArgs;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class AudioSearchViewModel : SearchViewModelBase<Audio>
    {
        public AudioSearchViewModel(
            InTouch inTouch, 
            INavigationService navigationService,
            ILocService locService, 
            ISettingsService settingsService, 
            IDialogsService dialogsService,
            IPlayerService playerService, 
            IAppLoaderService appLoaderService, 
            IDownloadsServiceHelper downloadsServiceHelper, 
            IInTouchWrapper inTouchWrapper,
            IPurchaseService purchaseService)
            : base(inTouch, navigationService, locService, settingsService, dialogsService, inTouchWrapper)
        {
            _playerService = playerService;
            _appLoaderService = appLoaderService;
            _downloadsServiceHelper = downloadsServiceHelper;
            _purchaseService = purchaseService;
            
            DownloadCommand = new DelegateCommand<Audio>(OnDownloadCommand);
            DownloadSelectedCommand = new DelegateCommand(OnDownloadSelectedCommand, HasSelectedItems);

            AddToMyAudiosCommand = new DelegateCommand<Audio>(OnAddToMyAudiosCommand, CanAddToMyAudios);
            AddSelectedToMyAudiosCommand = new DelegateCommand(OnAddSelectedToMyAudiosCommand, HasSelectedItems);

            DeleteCommand = new DelegateCommand<Audio>(OnDeleteCommand, CanDeleteAudio);
            DeleteSelectedCommand = new DelegateCommand(OnDeleteSelectedCommand, HasSelectedItems);

            PlaySelectedCommand = new DelegateCommand(OnPlaySelectedCommand, HasSelectedItems);
            ShowPerformerFlyoutCommand = new DelegateCommand(OnShowPerformerFlyoutCommand);
            ShowTrackInfoCommand = new DelegateCommand<Audio>(OnShowTrackInfoCommand);
        }

        [DoNotCheckEquality]
        public bool IsFilterFlyoutShowed { get; private set; }

        public bool PerformerOnly
        {
            get { return _settingsService.Get(PERFORMER_ONLY_PARAMETER_NAME, false); }
            set
            {
                _settingsService.Set(PERFORMER_ONLY_PARAMETER_NAME, value);
                Search();
            }
        }

        public bool LyricsOnly
        {
            get { return _settingsService.Get(LYRICS_ONLY_PARAMETER_NAME, false); }
            set
            {
                _settingsService.Set(LYRICS_ONLY_PARAMETER_NAME, value);
                Search();
            }
        }

        [DoNotNotify]
        public DelegateCommand<Audio> DownloadCommand { get; private set; }
        [DoNotNotify]
        public DelegateCommand<Audio> DeleteCommand { get; private set; }
        [DoNotNotify]
        public DelegateCommand<Audio> AddToMyAudiosCommand { get; private set; }
        [DoNotNotify]
        public DelegateCommand AddSelectedToMyAudiosCommand { get; private set; }
        [DoNotNotify]
        public DelegateCommand DownloadSelectedCommand { get; private set; }
        [DoNotNotify]
        public DelegateCommand DeleteSelectedCommand { get; private set; }
        [DoNotNotify]
        public DelegateCommand PlaySelectedCommand { get; private set; }
        [DoNotNotify]
        public DelegateCommand ShowPerformerFlyoutCommand { get; private set; }
        [DoNotNotify]
        public DelegateCommand<Audio> ShowTrackInfoCommand { get; private set; }

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

        protected override async Task<IEnumerable<Audio>> LoadMoreEverywhere(uint page)
        {
            if (String.IsNullOrWhiteSpace(_currentQuery))
                return new List<Audio>(0);

            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.Search(new AudioSearchParams
            {
                AutoComplete = true,
                Count = 50,
                Offset = _everywhereOffset,
                Query = _currentQuery,
                PerformerOnly = PerformerOnly,
                Lyrics = LyricsOnly
            }));

            if (response.IsError)
                throw new Exception(response.Error.ToString());

            _everywhereOffset += 50;
            return response.Data.Items;
        }

        protected override async Task<IEnumerable<Audio>> LoadMoreInCollection(uint page)
        {
            if (String.IsNullOrWhiteSpace(_currentQuery))
                return new List<Audio>(0);

            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.Search(new AudioSearchParams
            {
                AutoComplete = true,
                SearchOwn = true,
                Count = 50,
                Offset = _inCollectionOffset,
                Query = _currentQuery,
                PerformerOnly = PerformerOnly,
                Lyrics = LyricsOnly
            }));

            if (response.IsError)
                throw new Exception(response.Error.ToString());

            _inCollectionOffset += 50;
            return response.Data.Items.Where(a => a.OwnerId == UserId);
        }

        protected override void OnLastPivotIndexChanged()
        {
            base.OnLastPivotIndexChanged();
        }

        protected override void OnSelectionChangedCommand()
        {
            DownloadSelectedCommand.RaiseCanExecuteChanged();
            PlaySelectedCommand.RaiseCanExecuteChanged();
            AddSelectedToMyAudiosCommand.RaiseCanExecuteChanged();
            DeleteSelectedCommand.RaiseCanExecuteChanged();
        }

        protected override void CreateDefaultAppBarButtons()
        {
            base.CreateDefaultAppBarButtons();

            PrimaryItems.Add(new ButtonElement
            {
                Label = _locService["AppBarButton_Select_Text"],
                Icon = new FontButtonIcon() { Glyph = "\uE133", FontSize = 14 },
                Command = ActivateSelectionModeCommand
            });
            PrimaryItems.Add(new ButtonElement
            {
                Label = _locService["AppBarButton_Filter_Text"],
                Icon = new FontButtonIcon { Glyph = "\uE16E", FontSize = 14 },
                Command = ShowPerformerFlyoutCommand
            });
        }

        protected override void CreateSelectionAppBarButtons()
        {
            base.CreateSelectionAppBarButtons();

            PrimaryItems.Add(new ButtonElement
            {
                Label = _locService["AppBarButton_Download_Text"],
                Icon = new FontButtonIcon { Glyph = "\uE118" },
                Command = DownloadSelectedCommand
            });
            PrimaryItems.Add(new ButtonElement
            {
                Label = _locService["AppBarButton_Play_Text"],
                Icon = new FontButtonIcon { Glyph = "\uE102" },
                Command = PlaySelectedCommand
            });
            PrimaryItems.Add(new ButtonElement
            {
                Label = _locService["AppBarButton_SelectAll_Text"],
                Icon = new FontButtonIcon { Glyph = "\uE0E7" },
                Command = SelectAllCommand
            });

            if (LastPivotIndex == 1 && UserId == _inTouch.Session.UserId)
            {
                SecondaryItems.Add(new ButtonElement
                {
                    Label = _locService["AppBarButton_Delete_Text"],
                    Command = DeleteSelectedCommand
                });
            }
            else
            {
                SecondaryItems.Add(new ButtonElement
                {
                    Label = _locService["AppBarButton_AddToMyAudios_Text"],
                    Command = AddSelectedToMyAudiosCommand
                });
            }
        }

        protected override async void OnExecuteItemCommand(Audio item)
        {
            _appLoaderService.Show();

            if (LastPivotIndex == 0)
            {
                await _playerService.PlayNewTracks(EverywhereResults.ToPlayerTracks(),
                    EverywhereResults.IndexOf(item));
            }
            else
            {
                await _playerService.PlayNewTracks(InCollectionResults.ToPlayerTracks(),
                    InCollectionResults.IndexOf(item));
            }

            _navigationService.Navigate("PlayerView", null);
            _appLoaderService.Hide();
        }

        private void OnShowTrackInfoCommand(Audio track)
        {
            var info = new VKAudioInfo(track.Id, track.OwnerId, track.Title, track.Artist, track.Url, track.Duration);
            string parameter = JsonConvert.SerializeObject(info);

            if (_purchaseService.IsFullVersionPurchased)
                _navigationService.Navigate("VKAudioInfoView", parameter);
            else
                _navigationService.Navigate("PurchaseView", JsonConvert.SerializeObject(
                    new KeyValuePair<string, string>("VKAudioInfoView", parameter)));
        }

        private bool CanDeleteAudio(Audio audio)
        {
            return audio != null && audio.OwnerId == _inTouch.Session.UserId;
        }

        private bool CanAddToMyAudios(Audio audio)
        {
            return audio != null && audio.OwnerId != _inTouch.Session.UserId;
        }

        private void OnShowPerformerFlyoutCommand()
        {
            IsFilterFlyoutShowed = !IsFilterFlyoutShowed;
        }

        private async void OnDownloadCommand(Audio track)
        {
            await _downloadsServiceHelper.StartDownloadingAsync(track.ToDownloadable());
        }

        private bool HasSelectedItems()
        {
            return SelectedItems.Count > 0;
        }

        private async void OnPlaySelectedCommand()
        {
            _appLoaderService.Show();

            var toPlay = SelectedItems.Cast<Audio>().ToPlayerTracks();
            await _playerService.PlayNewTracks(toPlay, 0);
            _navigationService.Navigate("PlayerView", null);

            _appLoaderService.Hide();
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

        private async void OnDeleteCommand(Audio track)
        {
            _appLoaderService.Show(String.Format(_locService["AppLoader_DeletingItem"], track.ToString()));

            bool isSuccess = false;
            try
            {
                isSuccess = await DeleteAudio(track);
            }
            catch (Exception) { }

            if (!isSuccess)
            {
                _dialogsService.Show(_locService["Message_AudioDeleteError_Text"],
                    _locService["Message_AudioDeleteError_Title"]);
            }
            else if (LastPivotIndex == 1)
            {
                InCollectionResults.Remove(track);
            }

            _appLoaderService.Hide();
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

        private async void OnDeleteSelectedCommand()
        {
            _appLoaderService.Show(_locService["AppLoader_Preparing"]);
            _cancelOperations = false;

            var items = SelectedItems.Cast<Audio>().ToList();
            var errors = new List<Audio>();
            var success = new List<Audio>();

            foreach (var track in items)
            {
                if (_cancelOperations)
                {
                    errors.Add(track);
                    continue;
                }

                _appLoaderService.Show(String.Format(_locService["AppLoader_DeletingItem"], track.ToString()));

                bool isSuccess = false;
                try
                {
                    isSuccess = await DeleteAudio(track);
                }
                catch (Exception)
                {
                    errors.Add(track);
                    _cancelOperations = true;
                    _appLoaderService.Show(_locService["AppLoader_PleaseWait"]);
                    continue;
                }

                if (isSuccess)
                    success.Add(track);
                else
                    errors.Add(track);
                
                await Task.Delay(300);
            }

            RemoveDeletedItems(success);
            if (errors.Any())
                ShowDeletingError(errors);

            _appLoaderService.Hide();
            SetDefaultMode();
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
            SetDefaultMode();
        }

        private async Task<bool> AddToMyAudios(Audio audio)
        {
            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.Add(audio.Id, audio.OwnerId));

            if (response.IsCaptchaError())
                throw new Exception("Captcha error: cancel");
            return !response.IsError;
        }

        private async Task<bool> DeleteAudio(Audio audio)
        {
            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.Delete(audio.Id, audio.OwnerId));

            if (response.IsCaptchaError())
                throw new Exception("Captcha error: cancel");
            return !response.IsError;
        }

        private void RemoveDeletedItems(List<Audio> items)
        {
            if (LastPivotIndex == 1)
            {
                foreach (var audio in items)
                    InCollectionResults.Remove(audio);
            }
        }

        private void ShowAddingError(List<Audio> errorTracks)
        {
            var sb = new StringBuilder();
            sb.AppendLine(_locService["Message_AddSelectedError_Text"]);
            sb.AppendLine();

            foreach (var track in errorTracks)
                sb.AppendLine(track.ToString());

            _dialogsService.Show(sb.ToString(), _locService["Message_AddSelectedError_Title"]);
        }

        private void ShowDeletingError(List<Audio> errorTracks)
        {
            var sb = new StringBuilder();
            sb.AppendLine(_locService["Message_DeleteSelectedError_Text"]);
            sb.AppendLine();

            foreach (var track in errorTracks)
                sb.AppendLine(track.ToString());

            _dialogsService.Show(sb.ToString(), _locService["Message_DeleteSelectedError_Title"]);
        }

        private bool _cancelOperations = false;

        private readonly IPlayerService _playerService;
        private readonly IAppLoaderService _appLoaderService;
        private readonly IDownloadsServiceHelper _downloadsServiceHelper;
        private readonly IPurchaseService _purchaseService;

        internal const string PERFORMER_ONLY_PARAMETER_NAME = "AudioSearchPerformerOnly";
        internal const string LYRICS_ONLY_PARAMETER_NAME = "AudioSearchLyricsOnly";
    }
}
