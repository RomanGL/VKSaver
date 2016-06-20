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
            IAppLoaderService appLoaderService)
        {
            _navigationService = navigationService;
            _playerService = playerService;
            _downloadsServiceHelper = downloadsServiceHelper;
            _vkService = vkService;
            _appLoaderService = appLoaderService;

            IsItemClickEnabled = true;
            AppBarItems = new ObservableCollection<ICommandBarElement>();
            SelectedItems = new List<object>();

            PlayTracksCommand = new DelegateCommand<VKAudio>(OnPlayTracksCommand);
            DownloadTrackCommand = new DelegateCommand<VKAudio>(OnDownloadTrackCommand);
            DownloadSelectedCommand = new DelegateCommand(OnDownloadSelectedCommand, CanExecuteDownloadSelectedCommand);
            SelectionChangedCommand = new DelegateCommand(OnSelectionChangedCommand);
            ReloadContentCommand = new DelegateCommand(() => Tracks.Refresh());
            ActivateSelectionMode = new DelegateCommand(SetSelectionMode);
        }

        public PaginatedCollection<VKAudio> Tracks { get; private set; }

        public bool IsSelectionMode { get; private set; }

        public bool IsItemClickEnabled { get; private set; }

        [DoNotCheckEquality]
        public bool SelectAll { get; private set; }

        [DoNotNotify]
        public ObservableCollection<ICommandBarElement> AppBarItems { get; private set; }

        [DoNotNotify]
        public List<object> SelectedItems { get; private set; }

        [DoNotNotify]
        public DelegateCommand<VKAudio> PlayTracksCommand { get; private set; }

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
            if (e.NavigationMode == NavigationMode.New)
            {
                viewModelState[nameof(Tracks)] = JsonConvert.SerializeObject(Tracks.ToList());
                viewModelState[nameof(_offset)] = _offset;
            }

            AppBarItems.Clear();
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
            AppBarItems.Clear();

            AppBarItems.Add(new AppBarButton
            {
                Label = "обновить",
                Icon = new FontIcon { Glyph = "\uE117", FontSize = 14 },
                Command = ReloadContentCommand
            });
            AppBarItems.Add(new AppBarButton
            {
                Label = "выбрать",
                Icon = new FontIcon { Glyph = "\uE133", FontSize = 14 },
                Command = ActivateSelectionMode
            });
        }

        private void CreateSelectionAppBarButtons()
        {
            AppBarItems.Clear();
            AppBarItems.Add(new AppBarButton
            {
                Label = "загрузить",
                Icon = new FontIcon { Glyph = "\uE118" },
                Command = DownloadSelectedCommand
            });
            AppBarItems.Add(new AppBarButton
            {
                Label = "выбрать все",
                Icon = new FontIcon { Glyph = "\uE0E7" },
                Command = new DelegateCommand(() => SelectAll = !SelectAll)
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

        private async void OnPlayTracksCommand(VKAudio track)
        {
            await _playerService.PlayNewTracks(Tracks.ToPlayerTracks(), Tracks.IndexOf(track));
            _navigationService.Navigate("PlayerView", null);
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

        private bool CanExecuteDownloadSelectedCommand()
        {
            return SelectedItems.Count > 0;
        }

        private void OnSelectionChangedCommand()
        {
            DownloadSelectedCommand.RaiseCanExecuteChanged();
        }

        private uint _offset;

        private readonly INavigationService _navigationService;
        private readonly IPlayerService _playerService;
        private readonly IDownloadsServiceHelper _downloadsServiceHelper;
        private readonly IVKService _vkService;
        private readonly IAppLoaderService _appLoaderService;
    }
}
