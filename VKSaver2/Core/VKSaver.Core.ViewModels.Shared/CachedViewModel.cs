#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
using Windows.Storage.AccessCache;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Windows.Storage.AccessCache;
#elif ANDROID
using VKSaver.Core.Toolkit.Commands;
#endif

using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Collections;
using System.Collections;
using VKSaver.Core.Models.Common;
using VKSaver.Core.FileSystem;
using VKSaver.Core.Toolkit.Controls;
using VKSaver.Core.Toolkit.Navigation;
using NavigatedToEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatedToEventArgs;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class CachedViewModel : AudioViewModel<CachedTrack>
    {
        public CachedViewModel(
            IPlayerService playerService,
            ILocService locService,
            INavigationService navigationService,
            IAppLoaderService appLoaderService,
            IMusicCacheService musicCacheService,
            IDialogsService dialogsService)
            : base(playerService, locService, navigationService, appLoaderService)
        {
            _musicCacheService = musicCacheService;
            _dialogsService = dialogsService;

            DeleteTrackCommand = new DelegateCommand<CachedTrack>(OnDeleteTrackCommand);
            DeleteAllCommand = new DelegateCommand(OnDeleteAllCommand);
            DeleteSelectedCommand = new DelegateCommand(OnDeleteSelectedCommand, HasSelectedItems);
        }

        public PaginatedCollection<CachedTrack> CachedTracks { get; set; }

        [DoNotNotify]
        public DelegateCommand<CachedTrack> DeleteTrackCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand DeleteSelectedCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand DeleteAllCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (e.NavigationMode == NavigationMode.New || e.NavigationMode == NavigationMode.Refresh)
            {
                CachedTracks = new PaginatedCollection<CachedTrack>(LoadMoreTracks);
            }

            base.OnNavigatedTo(e, viewModelState);
        }

        protected override IList<CachedTrack> GetAudiosList()
        {
            return CachedTracks;
        }

        protected override IList GetSelectionList()
        {
            return CachedTracks;
        }

        protected override IPlayerTrack ConvertToPlayerTrack(CachedTrack track)
        {
            return track;
        }

        protected override void CreateDefaultAppBarButtons()
        {
            SecondaryItems.Add(new ButtonElement
            {
                Label = _locService["AppBarButton_DeleteAll_Text"],
                Command = DeleteAllCommand
            });

            base.CreateDefaultAppBarButtons();
        }

        protected override void CreateSelectionAppBarButtons()
        {
            SecondaryItems.Add(new ButtonElement
            {
                Label = _locService["AppBarButton_Delete_Text"],
                Command = DeleteSelectedCommand
            });

            base.CreateSelectionAppBarButtons();
        }

        protected override void PrepareTracksBeforePlay(IEnumerable<CachedTrack> tracks)
        {
#if WINDOWS_UWP || WINDOWS_PHONE_APP
            StorageApplicationPermissions.FutureAccessList.Clear();

            foreach (var track in tracks)
            {
                var file = (IWindowsFile)track.File.File;
                string token = StorageApplicationPermissions.FutureAccessList.Add(file.StorageFile);
                track.Source = $"vks-token:{token}";
            }
#elif ANDROID
            throw new NotImplementedException();
#endif
            base.PrepareTracksBeforePlay(tracks);
        }

        protected override void OnSelectionChangedCommand()
        {
            DeleteSelectedCommand.RaiseCanExecuteChanged();
            base.OnSelectionChangedCommand();
        }

        private async Task<IEnumerable<CachedTrack>> LoadMoreTracks(uint page)
        {
            if (page > 0)
                return new List<CachedTrack>(0);

            var files = await _musicCacheService.GetCachedFiles();
            var tracks = new List<CachedTrack>();

            foreach (var file in files)
            {
                var metadata = await file.GetMetadataAsync();
                var track = new CachedTrack
                {
                    File = file,
                    Title = metadata.Track.Title,
                    Artist = metadata.Track.Artist,
                    Duration = TimeSpan.FromTicks(metadata.Track.Duration),
                    VKInfo = metadata.VK
                };
                tracks.Add(track);
            }

            if (tracks.Count > 0)
                SetDefaultMode();

            return tracks;
        }

        private async void OnDeleteTrackCommand(CachedTrack track)
        {
            _appLoaderService.Show(String.Format(_locService["AppLoader_DeletingItem"], track.ToString()));

            try
            {
                await track.File.File.DeleteAsync();
                CachedTracks.Remove(track);
            }
            catch (Exception)
            {
                _dialogsService.Show(
                    _locService["Message_AudioDeleteError_Text"],
                    _locService["Message_AudioDeleteError_Title"]);
            }

            _appLoaderService.Hide();
        }

        private async void OnDeleteSelectedCommand()
        {
            _appLoaderService.Show(_locService["AppLoader_PleaseWait"]);

            var items = SelectedItems.Cast<CachedTrack>().ToList();
            SetDefaultMode();

            var errorTracks = new List<CachedTrack>();
            var successTracks = new List<CachedTrack>();

            foreach (var item in items)
            {
                try
                {
                    _appLoaderService.Show(String.Format(_locService["AppLoader_DeletingItem"], item.ToString()));
                    await item.File.File.DeleteAsync();
                }
                catch (Exception)
                {
                    errorTracks.Add(item);
                }
            }

            if (errorTracks.Count > 0)
                ShowDeletingError(errorTracks);

            if (successTracks.Count > 0)
            {
                _appLoaderService.Show(_locService["AppLoader_PleaseWait"]);
                RemoveSuccessItems(successTracks);
            }

            _appLoaderService.Hide();
        }

        private async void OnDeleteAllCommand()
        {
            _appLoaderService.Show(_locService["AppLoader_PleaseWait"]);
            bool success = await _musicCacheService.ClearMusicCache();

            if (success)
            {
                CachedTracks.Clear();
                CachedTracks.ContentState = ContentState.NoData;
            }
            else
            {
                _dialogsService.Show(
                    _locService["Message_ClearCacheError_Text"],
                    _locService["Message_ClearCacheError_Title"]);
            }

            _appLoaderService.Hide();
        }

        private void RemoveSuccessItems(List<CachedTrack> tracks)
        {
            foreach (var track in tracks)
            {
                CachedTracks.Remove(track);
            }

            if (CachedTracks.Count == 0)
                CachedTracks.ContentState = ContentState.NoData;
        }

        private void ShowDeletingError(List<CachedTrack> tracks)
        {
            var sb = new StringBuilder();
            sb.AppendLine(_locService["Message_DeleteSelectedError_Text"]);
            sb.AppendLine();

            foreach (var track in tracks)
                sb.AppendLine(track.ToString());

            _dialogsService.Show(sb.ToString(), _locService["Message_DeleteSelectedError_Title"]);
        }

        private readonly IMusicCacheService _musicCacheService;
        private readonly IDialogsService _dialogsService;
    }
}
