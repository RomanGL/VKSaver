using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using ModernDev.InTouch;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Collections;
using VKSaver.Core.ViewModels.Common;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class RecommendedViewModel : ViewModelBase
    {
        public RecommendedViewModel(InTouch inTouch, INavigationService navigationService,
            IPlayerService playerService, IDownloadsServiceHelper downloadsServiceHelper,
            IAppLoaderService appLoaderService, IDialogsService dialogsService,
            ILocService locService)
        {
            _inTouch = inTouch;
            _navigationService = navigationService;
            _playerService = playerService;
            _downloadsServiceHelper = downloadsServiceHelper;
            _appLoaderService = appLoaderService;
            _dialogsService = dialogsService;
            _locService = locService;

            IsItemClickEnabled = true;
            AppBarItems = new ObservableCollection<ICommandBarElement>();
            SecondaryItems = new ObservableCollection<ICommandBarElement>();
            SelectedItems = new List<object>();

            PlayTracksCommand = new DelegateCommand<Audio>(OnPlayTracksCommand);
            DownloadTrackCommand = new DelegateCommand<Audio>(OnDownloadTrackCommand);
            DownloadSelectedCommand = new DelegateCommand(OnDownloadSelectedCommand, HasSelectedItems);
            SelectionChangedCommand = new DelegateCommand(OnSelectionChangedCommand);
            ReloadContentCommand = new DelegateCommand(OnReloadContentCommand);
            ActivateSelectionMode = new DelegateCommand(SetSelectionMode);

            AddToMyAudiosCommand = new DelegateCommand<Audio>(OnAddToMyAudiosCommand);
            AddSelectedToMyAudiosCommand = new DelegateCommand(OnAddSelectedToMyAudiosCommand, HasSelectedItems);
            PlaySelectedCommand = new DelegateCommand(OnPlaySelectedCommand, HasSelectedItems);
        }

        public PaginatedCollection<Audio> Audios { get; private set; }

        public bool IsSelectionMode { get; private set; }

        public bool IsItemClickEnabled { get; private set; }

        [DoNotCheckEquality]
        public bool SelectAll { get; private set; }

        [DoNotNotify]
        public ObservableCollection<ICommandBarElement> AppBarItems { get; private set; }

        [DoNotNotify]
        public ObservableCollection<ICommandBarElement> SecondaryItems { get; private set; }

        [DoNotNotify]
        public List<object> SelectedItems { get; private set; }

        [DoNotNotify]
        public DelegateCommand<Audio> PlayTracksCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand PlaySelectedCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<Audio> DownloadTrackCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand SelectionChangedCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand DownloadSelectedCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand ReloadContentCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand ActivateSelectionMode { get; private set; }

        [DoNotNotify]
        public DelegateCommand AddSelectedToMyAudiosCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<Audio> AddToMyAudiosCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (e.Parameter != null)
                _userID = long.Parse(e.Parameter.ToString());

            if (viewModelState.Count > 0)
            {
                Audios = JsonConvert.DeserializeObject<PaginatedCollection<Audio>>
                    (viewModelState[nameof(Audios)].ToString());
                _audiosOffset = (int)viewModelState[nameof(_audiosOffset)];

                Audios.LoadMoreItems = LoadMoreAudios;
            }
            else
            {
                Audios = new PaginatedCollection<Audio>(LoadMoreAudios);
                _audiosOffset = 0;
            }

            if (Audios.Count > 0)
                SetDefaultMode();

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.Back && IsSelectionMode)
            {
                SetDefaultMode();
                e.Cancel = true;
                return;
            }

            if (e.NavigationMode == NavigationMode.New)
            {
                viewModelState[nameof(Audios)] = JsonConvert.SerializeObject(Audios.ToList());
                viewModelState[nameof(_audiosOffset)] = _audiosOffset;
            }

            AppBarItems.Clear();
            SecondaryItems.Clear();
            SelectedItems.Clear();
            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async Task<IEnumerable<Audio>> LoadMoreAudios(uint page)
        {
            var response = await _inTouch.Audio.GetRecommendations(
                userId: _userID > 0 ? (int?)_userID : null,
                count: 50,
                offset: _audiosOffset);

            if (response.IsError)
                throw new Exception(response.Error.ToString());
            else
            {
                if (!Audios.Any() && response.Data.Items.Any())
                    SetDefaultMode();

                _audiosOffset += 50;
                return response.Data.Items;
            }
        }

        private void CreateDefaultAppBarButtons()
        {
            AppBarItems.Clear();
            SecondaryItems.Clear();

            AppBarItems.Add(new AppBarButton
            {
                Label = _locService["AppBarButton_Refresh_Text"],
                Icon = new FontIcon { Glyph = "\uE117", FontSize = 14 },
                Command = ReloadContentCommand
            });
            AppBarItems.Add(new AppBarButton
            {
                Label = _locService["AppBarButton_Select_Text"],
                Icon = new FontIcon { Glyph = "\uE133", FontSize = 14 },
                Command = ActivateSelectionMode
            });
        }

        private void CreateSelectionAppBarButtons()
        {
            AppBarItems.Clear();
            SecondaryItems.Clear();

            AppBarItems.Add(new AppBarButton
            {
                Label = _locService["AppBarButton_Download_Text"],
                Icon = new FontIcon { Glyph = "\uE118" },
                Command = DownloadSelectedCommand
            });
            AppBarItems.Add(new AppBarButton
            {
                Label = _locService["AppBarButton_Play_Text"],
                Icon = new FontIcon { Glyph = "\uE102" },
                Command = PlaySelectedCommand
            });
            AppBarItems.Add(new AppBarButton
            {
                Label = _locService["AppBarButton_SelectAll_Text"],
                Icon = new FontIcon { Glyph = "\uE0E7" },
                Command = new DelegateCommand(() => SelectAll = !SelectAll)
            });            
            
            SecondaryItems.Add(new AppBarButton
            {
                Label = _locService["AppBarButton_AddToMyAudios_Text"],
                Command = AddSelectedToMyAudiosCommand
            });
        }

        private void SetSelectionMode()
        {
            IsSelectionMode = true;
            IsItemClickEnabled = false;

            CreateSelectionAppBarButtons();
        }

        private void SetDefaultMode()
        {
            IsSelectionMode = false;
            IsItemClickEnabled = true;

            CreateDefaultAppBarButtons();
        }

        private async void OnPlayTracksCommand(Audio track)
        {
            _appLoaderService.Show();

            await _playerService.PlayNewTracks(Audios.ToPlayerTracks(), Audios.IndexOf(track));
            _navigationService.Navigate("PlayerView", null);

            _appLoaderService.Hide();
        }

        private async void OnPlaySelectedCommand()
        {
            _appLoaderService.Show();

            var toPlay = SelectedItems.Cast<Audio>().ToPlayerTracks();
            await _playerService.PlayNewTracks(toPlay, 0);
            _navigationService.Navigate("PlayerView", null);

            _appLoaderService.Hide();
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

        private bool HasSelectedItems()
        {
            return SelectedItems.Count > 0;
        }

        private void OnSelectionChangedCommand()
        {
            DownloadSelectedCommand.RaiseCanExecuteChanged();
            PlaySelectedCommand.RaiseCanExecuteChanged();
            AddSelectedToMyAudiosCommand.RaiseCanExecuteChanged();
        }

        private void OnReloadContentCommand()
        {
            _audiosOffset = 0;
            Audios.Refresh();
        }

        private async void OnAddToMyAudiosCommand(Audio track)
        {
            _appLoaderService.Show(String.Format(_locService["AppLoader_AddingItem"], track.ToString()));
            if (!await AddToMyAudios(track))
            {
                _dialogsService.Show(_locService["Message_AudioAddError_Text"],
                    _locService["Message_AudioAddError_Title"]);
            }
            _appLoaderService.Hide();
        }

        private async void OnAddSelectedToMyAudiosCommand()
        {
            _appLoaderService.Show(_locService["AppLoader_Preparing"]);
            var items = SelectedItems.Cast<Audio>().ToList();
            var errors = new List<Audio>();

            foreach (var track in items)
            {
                _appLoaderService.Show(String.Format(_locService["AppLoader_AddingItem"], track.ToString()));
                if (!await AddToMyAudios(track))
                    errors.Add(track);
                await Task.Delay(200);
            }

            if (errors.Any())
                ShowAddingError(errors);

            _appLoaderService.Hide();
        }

        private async Task<bool> AddToMyAudios(Audio audio)
        {
            var response = await _inTouch.Audio.Add(audio.Id, audio.OwnerId);
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

        private int _audiosOffset;
        private long _userID;

        private readonly INavigationService _navigationService;
        private readonly IPlayerService _playerService;
        private readonly IDownloadsServiceHelper _downloadsServiceHelper;
        private readonly IAppLoaderService _appLoaderService;
        private readonly IDialogsService _dialogsService;
        private readonly ILocService _locService;
        private readonly InTouch _inTouch;
    }
}
