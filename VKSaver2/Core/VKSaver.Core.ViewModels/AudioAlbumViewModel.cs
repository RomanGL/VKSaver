using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Newtonsoft.Json;
using OneTeam.SDK.Core;
using OneTeam.SDK.VK.Models.Audio;
using OneTeam.SDK.VK.Models.Common;
using OneTeam.SDK.VK.Services.Interfaces;
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
    public sealed class AudioAlbumViewModel : ViewModelBase
    {
        public AudioAlbumViewModel(INavigationService navigationService, IPlayerService playerService,
            IDownloadsServiceHelper downloadsServiceHelper, IVKService vkService,
            IAppLoaderService appLoaderService, IVKLoginService vkLoginService,
            IDialogsService dialogsService)
        {
            _navigationService = navigationService;
            _playerService = playerService;
            _downloadsServiceHelper = downloadsServiceHelper;
            _vkService = vkService;
            _appLoaderService = appLoaderService;
            _vkLoginService = vkLoginService;
            _dialogsService = dialogsService;

            IsItemClickEnabled = true;
            PrimaryItems = new ObservableCollection<ICommandBarElement>();
            SecondaryItems = new ObservableCollection<ICommandBarElement>();
            SelectedItems = new List<object>();

            PlayTracksCommand = new DelegateCommand<VKAudio>(OnPlayTracksCommand);
            DownloadTrackCommand = new DelegateCommand<VKAudio>(OnDownloadTrackCommand);
            DownloadSelectedCommand = new DelegateCommand(OnDownloadSelectedCommand, HasSelectedItems);
            SelectionChangedCommand = new DelegateCommand(OnSelectionChangedCommand);
            ReloadContentCommand = new DelegateCommand(OnReloadContentCommand);
            ActivateSelectionMode = new DelegateCommand(SetSelectionMode);

            AddToMyAudiosCommand = new DelegateCommand<VKAudio>(OnAddToMyAudiosCommand, CanAddToMyAudios);
            AddSelectedToMyAudiosCommand = new DelegateCommand(OnAddSelectedToMyAudiosCommand, HasSelectedItems);
            PlaySelectedCommand = new DelegateCommand(OnPlaySelectedCommand, HasSelectedItems);

            DeleteAudioCommand = new DelegateCommand<VKAudio>(OnDeleteAudioCommand, CanDeleteAudio);
            DeleteSelectedCommand = new DelegateCommand(OnDeleteSelectedCommand, HasSelectedItems);
        }

        public PaginatedCollection<VKAudio> Tracks { get; private set; }

        public bool IsSelectionMode { get; private set; }

        public bool IsItemClickEnabled { get; private set; }

        [DoNotCheckEquality]
        public bool SelectAll { get; private set; }

        [DoNotNotify]
        public ObservableCollection<ICommandBarElement> PrimaryItems { get; private set; }

        [DoNotNotify]
        public ObservableCollection<ICommandBarElement> SecondaryItems { get; private set; }

        [DoNotNotify]
        public List<object> SelectedItems { get; private set; }

        [DoNotNotify]
        public DelegateCommand<VKAudio> PlayTracksCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand PlaySelectedCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<VKAudio> DownloadTrackCommand { get; private set; }

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
        public DelegateCommand<VKAudio> AddToMyAudiosCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand DeleteSelectedCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<VKAudio> DeleteAudioCommand { get; private set; }

        public VKAudioAlbum Album { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            Album = JsonConvert.DeserializeObject<VKAudioAlbum>(e.Parameter.ToString());

            if (viewModelState.Count > 0)
            {
                Tracks = JsonConvert.DeserializeObject<PaginatedCollection<VKAudio>>(
                    viewModelState[nameof(Tracks)].ToString());
                _offset = (uint)viewModelState[nameof(_offset)];

                Tracks.LoadMoreItems = LoadMoreAudios;
            }
            else
            {
                _offset = 0;
                Tracks = new PaginatedCollection<VKAudio>(LoadMoreAudios);
            }

            if (Tracks.Count > 0)
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
                viewModelState[nameof(Tracks)] = JsonConvert.SerializeObject(Tracks.ToList());
                viewModelState[nameof(_offset)] = _offset;
            }

            PrimaryItems.Clear();
            SecondaryItems.Clear();
            SelectedItems.Clear();
            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async Task<IEnumerable<VKAudio>> LoadMoreAudios(uint page)
        {
            var parameters = new Dictionary<string, string>
            {
                { "count", "50" },
                { "offset", _offset.ToString() },
                { "owner_id", Album.OwnerID.ToString() },
                { "album_id", Album.ID.ToString() }
            };

            var request = new Request<VKCountedItemsObject<VKAudio>>("audio.get", parameters);
            var response = await _vkService.ExecuteRequestAsync(request);

            if (response.IsSuccess)
            {
                if (Tracks.Count == 0 && response.Response.Items.Any())
                    SetDefaultMode();

                _offset += 50;
                return response.Response.Items;
            }
            else
                throw new Exception(response.Error.ToString());
        }

        private void CreateDefaultAppBarButtons()
        {
            PrimaryItems.Clear();
            SecondaryItems.Clear();

            PrimaryItems.Add(new AppBarButton
            {
                Label = "обновить",
                Icon = new FontIcon { Glyph = "\uE117", FontSize = 14 },
                Command = ReloadContentCommand
            });
            PrimaryItems.Add(new AppBarButton
            {
                Label = "выбрать",
                Icon = new FontIcon { Glyph = "\uE133", FontSize = 14 },
                Command = ActivateSelectionMode
            });
        }

        private void CreateSelectionAppBarButtons()
        {
            PrimaryItems.Clear();
            SecondaryItems.Clear();

            PrimaryItems.Add(new AppBarButton
            {
                Label = "загрузить",
                Icon = new FontIcon { Glyph = "\uE118" },
                Command = DownloadSelectedCommand
            });
            PrimaryItems.Add(new AppBarButton
            {
                Label = "воспроизвести",
                Icon = new FontIcon { Glyph = "\uE102" },
                Command = PlaySelectedCommand
            });
            PrimaryItems.Add(new AppBarButton
            {
                Label = "выбрать все",
                Icon = new FontIcon { Glyph = "\uE0E7" },
                Command = new DelegateCommand(() => SelectAll = !SelectAll)
            });

            if (Album.OwnerID != _vkLoginService.UserID)
            {
                SecondaryItems.Add(new AppBarButton
                {
                    Label = "в мои аудиозаписи",
                    Command = AddSelectedToMyAudiosCommand
                });                
            }
            else
            {
                SecondaryItems.Add(new AppBarButton
                {
                    Label = "удалить",
                    Command = DeleteSelectedCommand
                });
            }
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

        private async void OnPlayTracksCommand(VKAudio track)
        {
            _appLoaderService.Show();

            await _playerService.PlayNewTracks(Tracks.ToPlayerTracks(), Tracks.IndexOf(track));
            _navigationService.Navigate("PlayerView", null);

            _appLoaderService.Hide();
        }

        private async void OnPlaySelectedCommand()
        {
            _appLoaderService.Show();

            var toPlay = SelectedItems.Cast<VKAudio>().ToPlayerTracks();
            await _playerService.PlayNewTracks(toPlay, 0);
            _navigationService.Navigate("PlayerView", null);

            _appLoaderService.Hide();
        }

        private async void OnDownloadTrackCommand(VKAudio track)
        {
            await _downloadsServiceHelper.StartDownloadingAsync(track.ToDownloadable());
        }

        private async void OnDownloadSelectedCommand()
        {
            _appLoaderService.Show("Подготовка файлов для загрузки");
            var items = SelectedItems.ToList();
            SetDefaultMode();

            var toDownload = new List<IDownloadable>(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                toDownload.Add(((VKAudio)items[i]).ToDownloadable());
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
        }

        private void OnReloadContentCommand()
        {
            _offset = 0;
            Tracks.Refresh();
        }

        private bool CanAddToMyAudios(VKAudio audio)
        {
            return audio != null && audio.OwnerID != _vkLoginService.UserID;
        }

        private bool CanDeleteAudio(VKAudio audio)
        {
            return audio != null && audio.OwnerID == _vkLoginService.UserID;
        }

        private async void OnDeleteAudioCommand(VKAudio audio)
        {
            _appLoaderService.Show("Выполняется удаление...");
            if (!await DeleteAudio(audio))
            {
                _dialogsService.Show("При удалении аудиозаписи произошла ошибка. Повторите попытку позднее.",
                    "Не удалось удалить аудиозапись");
            }
            else
            {
                Tracks.Remove(audio);
            }

            _appLoaderService.Hide();
        }

        private async void OnAddToMyAudiosCommand(VKAudio audio)
        {
            _appLoaderService.Show("Выполняется добавление в вашу коллекцию...");
            if (!await AddToMyAudios(audio))
            {
                _dialogsService.Show("При добавлении аудиозаписи в коллекцию произошла ошибка. Повторите попытку позднее.",
                    "Не удалось добавить в коллекцию");
            }            
            _appLoaderService.Hide();
        }

        private async void OnAddSelectedToMyAudiosCommand()
        {
            _appLoaderService.Show("Подготовка...");
            var items = SelectedItems.Cast<VKAudio>().ToList();
            var errors = new List<VKAudio>();

            foreach (var track in items)
            {
                _appLoaderService.Show($"Выполняется добавление {track.Artist} - {track.Title}...");
                if (!await AddToMyAudios(track))
                    errors.Add(track);
                await Task.Delay(300);
            }

            if (errors.Any())
                ShowAddingError(errors);

            _appLoaderService.Hide();
        }

        private async void OnDeleteSelectedCommand()
        {
            _appLoaderService.Show("Подготовка...");
            var items = SelectedItems.Cast<VKAudio>().ToList();
            var errors = new List<VKAudio>();

            foreach (var track in items)
            {
                _appLoaderService.Show($"Выполняется удаление {track.Artist} - {track.Title}...");
                if (await DeleteAudio(track))
                    Tracks.Remove(track);
                else
                    errors.Add(track);

                await Task.Delay(300);
            }

            if (errors.Any())
                ShowDeletingError(errors);

            _appLoaderService.Hide();
        }

        private async Task<bool> AddToMyAudios(VKAudio audio)
        {
            var parameters = new Dictionary<string, string>
            {
                { "audio_id", audio.ID.ToString() },
                { "owner_id", audio.OwnerID.ToString() }
            };

            var request = new Request<long>("audio.add", parameters);
            var response = await _vkService.ExecuteRequestAsync(request);

            if (response.IsSuccess)
                return true;

            return false;
        }

        private async Task<bool> DeleteAudio(VKAudio audio)
        {
            var parameters = new Dictionary<string, string>
            {
                { "audio_id", audio.ID.ToString() },
                { "owner_id", audio.OwnerID.ToString() }
            };

            var request = new Request<VKOperationIsSuccess>("audio.delete", parameters);
            var response = await _vkService.ExecuteRequestAsync(request);

            if (response.IsSuccess)
                return true;

            return false;
        }

        private void ShowAddingError(List<VKAudio> errorTracks)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Не удалось добавить указанные аудиозаписи в вашу коллекцию. Повторите попытку позднее.");

            foreach (var track in errorTracks)
            {
                sb.AppendLine($"{track.Artist} - {track.Title}");
            }

            _dialogsService.Show(sb.ToString(), "Не удалось добавить в коллекцию");
        }

        private void ShowDeletingError(List<VKAudio> errorTracks)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Не удалось удалить указанные аудиозаписи из вашей коллекции. Повторите попытку позднее.");

            foreach (var track in errorTracks)
            {
                sb.AppendLine($"{track.Artist} - {track.Title}");
            }

            _dialogsService.Show(sb.ToString(), "Не удалось удалить");
        }

        private uint _offset;

        private readonly INavigationService _navigationService;
        private readonly IPlayerService _playerService;
        private readonly IDownloadsServiceHelper _downloadsServiceHelper;
        private readonly IVKService _vkService;
        private readonly IAppLoaderService _appLoaderService;
        private readonly IVKLoginService _vkLoginService;
        private readonly IDialogsService _dialogsService;
    }
}
