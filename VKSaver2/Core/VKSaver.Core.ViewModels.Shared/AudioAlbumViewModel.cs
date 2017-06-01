#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#else
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#endif

using ModernDev.InTouch;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Collections;
using VKSaver.Core.Services.Common;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class AudioAlbumViewModel : VKAudioImplementedViewModel
    {
        public AudioAlbumViewModel(
            INavigationService navigationService, 
            IPlayerService playerService,
            IDownloadsServiceHelper downloadsServiceHelper, 
            InTouch inTouch,
            IAppLoaderService appLoaderService, 
            IDialogsService dialogsService, 
            ILocService locService,
            IInTouchWrapper inTouchWrapper,
            IPurchaseService purchaseService) 
            : base(inTouch, appLoaderService, dialogsService, inTouchWrapper, 
                  downloadsServiceHelper, playerService, locService, navigationService, purchaseService)
        {
            IsReloadButtonSupported = true;

            DeleteAudioCommand = new DelegateCommand<Audio>(OnDeleteAudioCommand, CanDeleteAudio);
            DeleteSelectedCommand = new DelegateCommand(OnDeleteSelectedCommand, HasSelectedItems);
        }

        public double AudiosScrollPosition { get; set; }

        public AudioAlbum Album { get; private set; }

        public PaginatedCollection<Audio> Tracks { get; private set; }

        [DoNotNotify]
        public DelegateCommand DeleteSelectedCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<Audio> DeleteAudioCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            Album = JsonConvert.DeserializeObject<AudioAlbum>(e.Parameter.ToString());

            if (viewModelState.Count > 0)
            {
                Tracks = JsonConvert.DeserializeObject<PaginatedCollection<Audio>>(
                    viewModelState[nameof(Tracks)].ToString());
                _offset = (int)viewModelState[nameof(_offset)];

                Tracks.LoadMoreItems = LoadMoreAudios;

                object audiosScroll = null;
                viewModelState.TryGetValue(nameof(AudiosScrollPosition), out audiosScroll);

                if (audiosScroll != null)
                    AudiosScrollPosition = (double)audiosScroll;
            }
            else
            {
                _offset = 0;
                Tracks = new PaginatedCollection<Audio>(LoadMoreAudios);
            }

            if (Tracks.Count > 0)
                SetDefaultMode();

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                viewModelState[nameof(Tracks)] = JsonConvert.SerializeObject(Tracks.ToList());
                viewModelState[nameof(_offset)] = _offset;
                viewModelState[nameof(AudiosScrollPosition)] = AudiosScrollPosition;
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        protected override void OnReloadContentCommand()
        {
            _offset = 0;
            Tracks.Refresh();
        }

        protected override bool CanAddToMyAudios(Audio audio)
        {
            return audio != null && audio.OwnerId != _inTouch.Session.UserId;
        }

        protected override bool AddSelectedToMyAudiosSupported()
        {
            return Album.OwnerId != _inTouch.Session.UserId;
        }

        protected override IList GetSelectionList()
        {
            return Tracks;
        }

        protected override IList<Audio> GetAudiosList()
        {
            return Tracks;
        }

        protected override void CreateSelectionAppBarButtons()
        {
            if (!AddSelectedToMyAudiosSupported())
            {
                SecondaryItems.Add(new AppBarButton
                {
                    Label = _locService["AppBarButton_Delete_Text"],
                    Command = DeleteSelectedCommand
                });
            }

            base.CreateSelectionAppBarButtons();
        }

        protected override void OnSelectionChangedCommand()
        {
            base.OnSelectionChangedCommand();
            DeleteSelectedCommand.RaiseCanExecuteChanged();
        }

        private bool CanDeleteAudio(Audio audio)
        {
            return audio != null && audio.OwnerId == _inTouch.Session.UserId;
        }

        private async Task<IEnumerable<Audio>> LoadMoreAudios(uint page)
        {
            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.Get(
                Album.OwnerId, (int)Album.Id, count: 50, offset: _offset));

            if (response.IsError)
                throw new Exception(response.Error.ToString());

            if (!Tracks.Any() && response.Data.Items.Any())
                SetDefaultMode();

            _offset += 50;
            return response.Data.Items;
        }

        private async void OnDeleteAudioCommand(Audio audio)
        {
            _appLoaderService.Show(String.Format(_locService["AppLoader_DeletingItem"], audio.ToString()));

            bool isSuccess = false;
            try
            {
                isSuccess = await DeleteAudio(audio);
            }
            catch { }

            if (!isSuccess)
            {
                _dialogsService.Show(_locService["Message_AudioDeleteError_Text"],
                    _locService["Message_AudioDeleteError_Title"]);
            }
            else
            {
                Tracks.Remove(audio);
            }

            _appLoaderService.Hide();
        }

        private async void OnDeleteSelectedCommand()
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

                _appLoaderService.Show(String.Format(_locService["AppLoader_DeletingItem"], track));

                bool isSuccess;
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
                    Tracks.Remove(track);
                else
                    errors.Add(track);

                await Task.Delay(300);
                
            }

            if (errors.Any())
                ShowDeletingError(errors);

            _appLoaderService.Hide();
        }

        private async Task<bool> DeleteAudio(Audio audio)
        {
            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.Delete(
                audio.Id, audio.OwnerId));

            if (response.IsCaptchaError())
                throw new Exception("Captcha error: cancel");
            return !response.IsError;
        }

        private void ShowDeletingError(List<Audio> errorTracks)
        {
            var sb = new StringBuilder();
            sb.AppendLine(_locService["Message_DeleteSelectedError_Text"]);
            sb.AppendLine();

            foreach (var track in errorTracks)
            {
                sb.AppendLine(track.ToString());
            }

            _dialogsService.Show(sb.ToString(), _locService["Message_DeleteSelectedError_Title"]);
        }

        private int _offset;
    }
}
