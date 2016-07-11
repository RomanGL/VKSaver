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
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class VideoSearchViewModel : SearchViewModelBase<Video>
    {
        public VideoSearchViewModel(InTouch inTouch, INavigationService navigationService,
            ILocService locService, ISettingsService settingsService, IDialogsService dialogsService,
            IAppLoaderService appLoaderService, IInTouchWrapper inTouchWrapper)
            : base(inTouch, navigationService, locService, settingsService, dialogsService, inTouchWrapper)
        {
            _appLoaderService = appLoaderService;

            AddToMyVideosCommand = new DelegateCommand<Video>(OnAddToMyVideosCommand, CanAddToMyVideos);
            AddSelectedToMyVideosCommand = new DelegateCommand(OnAddSelectedToMyAudiosCommand, HasSelectedItems);

            DeleteCommand = new DelegateCommand<Video>(OnDeleteCommand, CanDeleteVideo);
            DeleteSelectedCommand = new DelegateCommand(OnDeleteSelectedCommand, HasSelectedItems);
            ShowFilterFlyoutCommand = new DelegateCommand(OnShowPerformerFlyoutCommand);
            FilterFlyoutClosedCommand = new DelegateCommand(OnFilterFlyoutClosedCommand);
            CreateFilters();
        }

        [DoNotCheckEquality]
        public bool IsFilterFlyoutShowed { get; private set; }

        public bool SafeSearch
        {
            get { return _settingsService.Get(SAFE_SEARCH_PARAMETER_NAME, true); }
            set
            {
                _settingsService.Set(SAFE_SEARCH_PARAMETER_NAME, value);
                _isFilterChanged = true;
            }
        }

        public bool OnlyHD
        {
            get { return _settingsService.Get(ONLY_HD_PARAMETER_NAME, false); }
            set
            {
                _settingsService.Set(ONLY_HD_PARAMETER_NAME, value);
                _isFilterChanged = true;
            }
        }

        public int SelectedSortMethod
        {
            get { return _settingsService.Get(SORT_METHOD_PARAMETER_NAME, 2); }
            set
            {
                _settingsService.Set(SORT_METHOD_PARAMETER_NAME, value);
                _isFilterChanged = true;
            }
        }

        public int SelectedVideoType
        {
            get { return _settingsService.Get(VIDEO_TYPE_PARAMETER_NAME, 0); }
            set
            {
                _settingsService.Set(VIDEO_TYPE_PARAMETER_NAME, value);
                _isFilterChanged = true;
            }
        }

        public int SelectedVideoDuration
        {
            get { return _settingsService.Get(VIDEO_DURATION_PARAMETER_NAME, 0); }
            set
            {
                _settingsService.Set(VIDEO_DURATION_PARAMETER_NAME, value);
                _isFilterChanged = true;
            }
        }

        [DoNotNotify]
        public List<VideoFilterItem> VideoTypes { get; private set; }
        [DoNotNotify]
        public List<VideoFilterItem> VideoDurations { get; private set; }
        [DoNotNotify]
        public List<VideoSortMethodItem> VideoSortMethods { get; private set; }

        [DoNotNotify]
        public DelegateCommand<Video> DownloadCommand { get; private set; }
        [DoNotNotify]
        public DelegateCommand<Video> DeleteCommand { get; private set; }
        [DoNotNotify]
        public DelegateCommand<Video> AddToMyVideosCommand { get; private set; }
        [DoNotNotify]
        public DelegateCommand AddSelectedToMyVideosCommand { get; private set; }
        [DoNotNotify]
        public DelegateCommand DeleteSelectedCommand { get; private set; }
        [DoNotNotify]
        public DelegateCommand ShowFilterFlyoutCommand { get; private set; }
        [DoNotNotify]
        public DelegateCommand FilterFlyoutClosedCommand { get; private set; }

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

        protected override async Task<IEnumerable<Video>> LoadMoreEverywhere(uint page)
        {
            if (String.IsNullOrWhiteSpace(_currentQuery))
                return new List<Video>(0);

            var filters = new List<VideoSearchFilters>(2);
            if (SelectedVideoType != 0)
                filters.Add(VideoTypes[SelectedVideoType].Filter.Value);
            if (SelectedVideoDuration != 0)
                filters.Add(VideoDurations[SelectedVideoDuration].Filter.Value);

            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Videos.Search(new VideoSearchParams
            {
                Query = _currentQuery,
                Count = 50,
                Offset = _everywhereOffset,
                Filters = filters,
                UnsafeSearch = !SafeSearch,
                NeedHD = OnlyHD,
                Sort = (int)VideoSortMethods[SelectedSortMethod].Method
            }));

            if (response.IsError)
                throw new Exception(response.Error.ToString());

            _everywhereOffset += 50;
            return response.Data.Items;
        }

        protected override async Task<IEnumerable<Video>> LoadMoreInCollection(uint page)
        {
            if (String.IsNullOrWhiteSpace(_currentQuery))
                return new List<Video>(0);

            var filters = new List<VideoSearchFilters>(2);
            if (SelectedVideoType != 0)
                filters.Add(VideoTypes[SelectedVideoType].Filter.Value);
            if (SelectedVideoDuration != 0)
                filters.Add(VideoDurations[SelectedVideoDuration].Filter.Value);

            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Videos.Search(new VideoSearchParams
            {
                Query = _currentQuery,
                Count = 50,
                Offset = _inCollectionOffset,
                OnlyOwn = true,
                Filters = filters,
                UnsafeSearch = !SafeSearch,
                NeedHD = OnlyHD,
                Sort = (int)VideoSortMethods[SelectedSortMethod].Method
            }));

            if (response.IsError)
                throw new Exception(response.Error.ToString());

            _inCollectionOffset += 50;
            return response.Data.Items.Where(v => v.OwnerId == UserId);
        }

        protected override void OnLastPivotIndexChanged()
        {
            base.OnLastPivotIndexChanged();
        }

        protected override void OnSelectionChangedCommand()
        {
            AddSelectedToMyVideosCommand.RaiseCanExecuteChanged();
            DeleteSelectedCommand.RaiseCanExecuteChanged();
        }

        protected override void CreateDefaultAppBarButtons()
        {
            base.CreateDefaultAppBarButtons();

            PrimaryItems.Add(new AppBarButton
            {
                Label = _locService["AppBarButton_Select_Text"],
                Icon = new FontIcon { Glyph = "\uE133", FontSize = 14 },
                Command = ActivateSelectionModeCommand
            });
            PrimaryItems.Add(new AppBarButton
            {
                Label = _locService["AppBarButton_Filter_Text"],
                Icon = new FontIcon { Glyph = "\uE16E", FontSize = 14 },
                Command = ShowFilterFlyoutCommand
            });
        }

        protected override void CreateSelectionAppBarButtons()
        {
            base.CreateSelectionAppBarButtons();

            PrimaryItems.Add(new AppBarButton
            {
                Label = _locService["AppBarButton_SelectAll_Text"],
                Icon = new FontIcon { Glyph = "\uE0E7" },
                Command = SelectAllCommand
            });

            if (LastPivotIndex == 1 && UserId == _inTouch.Session.UserId)
            {
                SecondaryItems.Add(new AppBarButton
                {
                    Label = _locService["AppBarButton_Delete_Text"],
                    Command = DeleteSelectedCommand
                });
            }
            else
            {
                SecondaryItems.Add(new AppBarButton
                {
                    Label = _locService["AppBarButton_AddToMyVideos_Text"],
                    Command = AddSelectedToMyVideosCommand
                });
            }
        }

        protected override void OnExecuteItemCommand(Video item)
        {
            _navigationService.Navigate("VideoInfoView", JsonConvert.SerializeObject(item));
        }

        private void CreateFilters()
        {
            VideoTypes = new List<VideoFilterItem>
            {
                new VideoFilterItem { Title = _locService["VideoSearchView_Filters_All_Text"] },
                new VideoFilterItem { Title = _locService["VideoSearchView_VideoTypes_MP4_Text"], Filter = VideoSearchFilters.Mp4 },
                new VideoFilterItem { Title = _locService["VideoSearchView_VideoTypes_YouTube_Text"], Filter = VideoSearchFilters.Youtube },
                new VideoFilterItem { Title = _locService["VideoSearchView_VideoTypes_Vimeo_Text"], Filter = VideoSearchFilters.Vimeo }
            };
            VideoDurations = new List<VideoFilterItem>
            {
                new VideoFilterItem { Title = _locService["VideoSearchView_Filters_All_Text"] },
                new VideoFilterItem { Title = _locService["VideoSearchView_VideoDurations_Short_Text"], Filter = VideoSearchFilters.Short },
                new VideoFilterItem { Title = _locService["VideoSearchView_VideoDurations_Long_Text"], Filter = VideoSearchFilters.Long }
            };
            VideoSortMethods = new List<VideoSortMethodItem>
            {
                new VideoSortMethodItem { Title = _locService["VideoSearchView_VideoSortMethods_ByDate_Text"], Method = VideoSortMethod.ByDate },
                new VideoSortMethodItem { Title = _locService["VideoSearchView_VideoSortMethods_ByDuration_Text"], Method = VideoSortMethod.ByDuration },
                new VideoSortMethodItem { Title = _locService["VideoSearchView_VideoSortMethods_ByRelevance_Text"], Method = VideoSortMethod.ByRelevance }
            };
        }

        private void OnShowPerformerFlyoutCommand()
        {
            IsFilterFlyoutShowed = !IsFilterFlyoutShowed;
        }

        private bool HasSelectedItems()
        {
            return SelectedItems.Count > 0;
        }

        private bool CanDeleteVideo(Video video)
        {
            return video != null && LastPivotIndex == 1 && UserId == _inTouch.Session.UserId;
        }

        private bool CanAddToMyVideos(Video video)
        {
            return video != null && !(LastPivotIndex == 1 && UserId == _inTouch.Session.UserId);
        }

        private void OnFilterFlyoutClosedCommand()
        {
            if (_isFilterChanged)
            {
                _isFilterChanged = false;
                Search();
            }
        }

        private async void OnDeleteCommand(Video video)
        {
            _appLoaderService.Show(String.Format(_locService["AppLoader_DeletingItem"], video.ToString()));
            bool success = await DeleteVideo(video);

            if (!success)
            {
                _dialogsService.Show(_locService["Message_VideoDeleteError_Text"],
                    _locService["Message_VideoDeleteError_Title"]);
            }
            else if (LastPivotIndex == 1)
            {
                InCollectionResults.Remove(video);
            }

            _appLoaderService.Hide();
        }

        private async void OnAddToMyVideosCommand(Video video)
        {
            _appLoaderService.Show(String.Format(_locService["AppLoader_AddingItem"], video.ToString()));
            if (!await AddToMyVideos(video))
            {
                _dialogsService.Show(_locService["Message_VideoAddError_Text"],
                    _locService["Message_VideoAddError_Title"]);
            }
            _appLoaderService.Hide();
        }

        private async void OnDeleteSelectedCommand()
        {
            _appLoaderService.Show(_locService["AppLoader_Preparing"]);
            _cancelOperations = false;

            var items = SelectedItems.Cast<Video>().ToList();
            var errors = new List<Video>();
            var success = new List<Video>();

            foreach (var track in items)
            {
                if (_cancelOperations)
                {
                    errors.Add(track);
                    continue;
                }

                _appLoaderService.Show(String.Format(_locService["AppLoader_DeletingItem"], track.ToString()));
                if (await DeleteVideo(track))
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

            var items = SelectedItems.Cast<Video>().ToList();
            var errors = new List<Video>();

            foreach (var track in items)
            {
                if (_cancelOperations)
                {
                    errors.Add(track);
                    continue;
                }

                _appLoaderService.Show(String.Format(_locService["AppLoader_AddingItem"], track.ToString()));
                if (!await AddToMyVideos(track))
                    errors.Add(track);

                await Task.Delay(200);
            }

            if (errors.Any())
                ShowAddingError(errors);

            _appLoaderService.Hide();
            SetDefaultMode();
        }

        private async Task<bool> AddToMyVideos(Video video)
        {
            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Videos.Add(
                (int)video.Id, video.OwnerId, _inTouch.Session.UserId));
            return !response.IsError && response.Data;
        }

        private async Task<bool> DeleteVideo(Video video)
        {
            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Videos.Delete(
                (int)video.Id, video.OwnerId, _inTouch.Session.UserId));
            return !response.IsError && response.Data;
        }

        private void RemoveDeletedItems(List<Video> items)
        {
            if (LastPivotIndex == 1)
            {
                foreach (var video in items)
                    InCollectionResults.Remove(video);
            }
        }

        private void ShowAddingError(List<Video> errorTracks)
        {
            var sb = new StringBuilder();
            sb.AppendLine(_locService["Message_AddSelectedError_Text"]);
            sb.AppendLine();

            foreach (var video in errorTracks)
                sb.AppendLine(video.ToString());

            _dialogsService.Show(sb.ToString(), _locService["Message_AddSelectedError_Title"]);
        }

        private void ShowDeletingError(List<Video> errorTracks)
        {
            var sb = new StringBuilder();
            sb.AppendLine(_locService["Message_DeleteSelectedError_Text"]);
            sb.AppendLine();

            foreach (var video in errorTracks)
                sb.AppendLine(video.ToString());

            _dialogsService.Show(sb.ToString(), _locService["Message_DeleteSelectedError_Title"]);
        }

        private bool _cancelOperations = false;
        private bool _isFilterChanged = false;

        private readonly IAppLoaderService _appLoaderService;

        private const string SAFE_SEARCH_PARAMETER_NAME = "VideoSearchSafeSearch";
        private const string ONLY_HD_PARAMETER_NAME = "VideoSearchOnlyHD";
        private const string SORT_METHOD_PARAMETER_NAME = "VideoSearchSortMethod";
        private const string VIDEO_TYPE_PARAMETER_NAME = "VideoSearchVideoType";
        private const string VIDEO_DURATION_PARAMETER_NAME = "VideoSearchVideoDuration";

        public sealed class VideoFilterItem
        {
            public string Title { get; set; }
            public VideoSearchFilters? Filter { get; set; }
        }

        public sealed class VideoSortMethodItem
        {
            public string Title { get; set; }
            public VideoSortMethod Method { get; set; }
        }

        public enum VideoSortMethod
        {
            ByDate = 0,
            ByDuration = 1,
            ByRelevance = 2
        }
    }
}
