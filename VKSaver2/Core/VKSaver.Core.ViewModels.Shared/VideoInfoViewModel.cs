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
using System.Collections.Generic;
using System.Linq;
using VKSaver.Core.LinksExtractor;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Common;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class VideoInfoViewModel : ViewModelBase
    {
        public VideoInfoViewModel(INavigationService navigationService, InTouch inTouch,
            IAppLoaderService appLoaderService, ILocService locService,
            IVideoLinksExtractor videoLinksExtractor, IDialogsService dialogsService,
            IDownloadsServiceHelper downloadsServiceHelper)
        {
            _navigationService = navigationService;
            _inTouch = inTouch;
            _appLoaderService = appLoaderService;
            _locService = locService;
            _videoLinksExtractor = videoLinksExtractor;
            _dialogsService = dialogsService;
            _downloadsServiceHelper = downloadsServiceHelper;

            LoadLinksCommand = new DelegateCommand(OnLoadLinksCommand);
            PlayVideoCommand = new DelegateCommand(OnPlayVideoCommand, CanPlayAndDownload);
            DownloadVideoCommand = new DelegateCommand(OnDownloadVideoCommand, CanPlayAndDownload);
        }

        public ContentState LinksState { get; private set; }

        public List<IVideoLink> VideoLinks { get; private set; }

        public int SelectedLinkIndex
        {
            get { return _selectedLinkIndex; }
            set
            {
                _selectedLinkIndex = value;
                PlayVideoCommand.RaiseCanExecuteChanged();
                DownloadVideoCommand.RaiseCanExecuteChanged();
            }
        }

        public Video Video { get; private set; }

        [DependsOn(nameof(Video))]
        public string VideoImage
        {
            get
            {
                if (Video == null)
                    return null;

                if (!String.IsNullOrEmpty(Video.Photo800))
                    return Video.Photo800;
                else if (!String.IsNullOrEmpty(Video.Photo640))
                    return Video.Photo640;
                else if (!String.IsNullOrEmpty(Video.Photo320))
                    return Video.Photo320;
                else
                    return Video.Photo130;
            }
        }

        public string VideoStoresOn { get; private set; }

        [DoNotNotify]
        public DelegateCommand LoadLinksCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand PlayVideoCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand DownloadVideoCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            Video = JsonConvert.DeserializeObject<Video>(e.Parameter.ToString());

            if (viewModelState.Count > 0)
            {
                VideoLinks = JsonConvert.DeserializeObject<List<CommonVideoLink>>(
                    viewModelState[nameof(VideoLinks)].ToString()).Cast<IVideoLink>().ToList();
                SelectedLinkIndex = (int)viewModelState[nameof(SelectedLinkIndex)];
            }
            else
            {
                SelectedLinkIndex = 0;
                VideoLinks = null;
            }

            UpdateState();
            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                viewModelState[nameof(VideoLinks)] = JsonConvert.SerializeObject(VideoLinks);
                viewModelState[nameof(SelectedLinkIndex)] = SelectedLinkIndex;
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private void UpdateState()
        {
            if (TrySetVkVideos())
            {
                SelectedLinkIndex = 0;
            }
            else if (String.IsNullOrEmpty(Video.Player))
            {
                LinksState = ContentState.NoData;
                VideoStoresOn = _locService["VideoInfoView_StoresOn_Unsupported_Text"];
                return;
            }
            else if (Video.Player.Contains("vk.com"))
                VideoStoresOn = _locService["VideoInfoView_StoresOn_VK_Text"];
            else if (Video.Player.Contains("youtube"))
                VideoStoresOn = _locService["VideoInfoView_StoresOn_YouTube_Text"];
            else if (Video.Player.Contains("vimeo.com"))
                VideoStoresOn = _locService["VideoInfoView_StoresOn_Vimeo_Text"];
            //else if (Video.Player.Contains("coub.com"))
            //    VideoStoresOn = _locService["VideoInfoView_StoresOn_Coub_Text"];
            else
            {
                VideoStoresOn = _locService["VideoInfoView_StoresOn_Unsupported_Text"];
                LinksState = ContentState.NoData;
                return;
            }

            if (VideoLinks != null)
                LinksState = ContentState.Normal;
            else
                LinksState = ContentState.Error;
        }

        private bool TrySetVkVideos()
        {
            if (Video.Files != null && Video.Files.Vid240 != null)
            {
                VideoLinks = new List<IVideoLink>();
                VideoLinks.Add(new CommonVideoLink { Name = "MP4 240p", Source = Video.Files.Vid240 });

                if (Video.Files.Vid360 != null)
                    VideoLinks.Add(new CommonVideoLink { Name = "MP4 360p", Source = Video.Files.Vid360 });
                if (Video.Files.Vid480 != null)
                    VideoLinks.Add(new CommonVideoLink { Name = "MP4 480p", Source = Video.Files.Vid480 });
                if (Video.Files.Vid720 != null)
                    VideoLinks.Add(new CommonVideoLink { Name = "MP4 720p", Source = Video.Files.Vid720 });
                if (Video.Files.Vid1080 != null)
                    VideoLinks.Add(new CommonVideoLink { Name = "MP4 1080p", Source = Video.Files.Vid1080 });

                return true;
            }

            return false;
        }

        private async void OnLoadLinksCommand()
        {
            LinksState = ContentState.Loading;

            try
            {
                var videos = await _videoLinksExtractor.GetLinks(Video.Player);
                if (videos != null)
                    VideoLinks = videos;
            }
            catch (Exception)
            {
                LinksState = ContentState.Error;

                _dialogsService.Show(_locService["Message_VideoInfo_GetLinksFailed_Text"],
                    _locService["Message_VideoInfo_GetLinksFailed_Title"]);

                return;
            }

            SelectedLinkIndex = 0;
            LinksState = ContentState.Normal;
        }

        private bool CanPlayAndDownload()
        {
            return SelectedLinkIndex != -1;
        }

        private void OnPlayVideoCommand()
        {
            var data = new KeyValuePair<int, List<IVideoLink>>(SelectedLinkIndex, VideoLinks);
            _navigationService.Navigate("VideoPlayerView", JsonConvert.SerializeObject(data));
        }

        private void OnDownloadVideoCommand()
        {
            _downloadsServiceHelper.StartDownloadingAsync(VideoLinks[SelectedLinkIndex].ToDownloadable(Video.Title));
        }

        private int _selectedLinkIndex;

        private readonly INavigationService _navigationService;
        private readonly IAppLoaderService _appLoaderService;
        private readonly ILocService _locService;
        private readonly IVideoLinksExtractor _videoLinksExtractor;
        private readonly InTouch _inTouch;
        private readonly IDialogsService _dialogsService;
        private readonly IDownloadsServiceHelper _downloadsServiceHelper;
    }
}
