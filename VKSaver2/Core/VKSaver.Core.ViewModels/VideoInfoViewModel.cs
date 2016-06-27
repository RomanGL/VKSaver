using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Newtonsoft.Json;
using OneTeam.SDK.VK.Models.Video;
using OneTeam.SDK.VK.Services.Interfaces;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.LinksExtractor;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class VideoInfoViewModel : ViewModelBase
    {
        public VideoInfoViewModel(INavigationService navigationService, IVKService vkService,
            IAppLoaderService appLoaderService, ILocService locService,
            IVideoLinksExtractor videoLinksExtractor)
        {
            _navigationService = navigationService;
            _vkService = vkService;
            _appLoaderService = appLoaderService;
            _locService = locService;
            _videoLinksExtractor = videoLinksExtractor;

            LoadLinksCommand = new DelegateCommand(OnLoadLinksCommand);
        }

        public ContentState LinksState { get; private set; }

        public List<IVideoLink> VideoLinks { get; private set; }

        public int SelectedLinkIndex { get; set; }

        public VKVideo Video { get; private set; }

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

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            Video = JsonConvert.DeserializeObject<VKVideo>(e.Parameter.ToString());

            if (viewModelState.Count > 0)
            {

            }
            else
            {
            }

            UpdateState();
            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private void UpdateState()
        {
            if (String.IsNullOrEmpty(Video.Player))
            {
                LinksState = ContentState.NoData;
                VideoStoresOn = _locService["VideoInfoView_StoresOn_Unsupported_Text"];
            }

            if (Video.Player.Contains("vk.com"))
                VideoStoresOn = _locService["VideoInfoView_StoresOn_VK_Text"];
            //else if (Video.Player.Contains("youtube"))
            //    VideoStoresOn = _locService["VideoInfoView_StoresOn_YouTube_Text"];
            //else if (Video.Player.Contains("coub.com"))
            //    VideoStoresOn = _locService["VideoInfoView_StoresOn_Coub_Text"];
            //else if (Video.Player.Contains("vimeo.com"))
            //    VideoStoresOn = _locService["VideoInfoView_StoresOn_Vimeo_Text"];
            else
            {
                LinksState = ContentState.NoData;
                VideoStoresOn = _locService["VideoInfoView_StoresOn_Unsupported_Text"];
                return;
            }

            LinksState = ContentState.Error;
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
                return;
            }

            SelectedLinkIndex = 0;
            LinksState = ContentState.Normal;
        }

        private readonly INavigationService _navigationService;
        private readonly IVKService _vkService;
        private readonly IAppLoaderService _appLoaderService;
        private readonly ILocService _locService;
        private readonly IVideoLinksExtractor _videoLinksExtractor;
    }
}
