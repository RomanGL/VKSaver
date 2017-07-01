using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using IF.Lastfm.Core.Api;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using ModernDev.InTouch;
using Newtonsoft.Json;
using PropertyChanged;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Common;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class SpecialMainViewModel : BaseMainViewModel
    {
        public SpecialMainViewModel(
            InTouch inTouch, 
            LastfmClient lfClient, 
            IInTouchWrapper inTouchWrapper, 
            INavigationService navigationService, 
            IDownloadsServiceHelper downloadsServiceHelper, 
            IPurchaseService purchaseService,
            IAdsService adsService,
            IPlayerService playerService,
            IImagesCacheService imagesCacheService,
            IDialogsService dialogsService,
            ILocService locService,
            IAppLoaderService appLoaderService) 
            : base(inTouch, 
                  lfClient, 
                  inTouchWrapper, 
                  navigationService, 
                  downloadsServiceHelper, 
                  purchaseService,
                  adsService,
                  playerService,
                  imagesCacheService)
        {
            _dialogsService = dialogsService;
            _locService = locService;
            _appLoaderService = appLoaderService;

            ReloadCatalogCommand = new DelegateCommand(OnReloadCatalogCommand);
            PlaySpecialCommand = new DelegateCommand<Audio>(OnPlaySpecialCommand);
            PlayNewAudiosCommand = new DelegateCommand<Audio>(OnPlayNewAudiosCommand);
            AddToMyAudiosCommand = new DelegateCommand<Audio>(OnAddToMyAudiosCommand, CanAddToMyAudios);
            ShowTrackInfoCommand = new DelegateCommand<Audio>(OnShowTrackInfoCommand);
        }

        public string HubBackgroundImage { get; private set; }

        public ContentState CatalogState { get; private set; }

        public AudioCatalogItem SpecialItem { get; private set; }

        public AudioCatalogItem NewAudiosItem { get; private set; }

        [DoNotNotify]
        public DelegateCommand ReloadCatalogCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<Audio> PlaySpecialCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<Audio> PlayNewAudiosCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<Audio> AddToMyAudiosCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<Audio> ShowTrackInfoCommand { get; private set; }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            _backgroundLoaded = false;

            await LoadCatalogAsync();

            base.OnNavigatedTo(e, viewModelState);
            await Task.Delay(2000);
            _adsService.ActivateAds();
        }

        private async Task LoadCatalogAsync()
        {
            CatalogState = ContentState.Loading;
            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.GetCatalog());
            if (response.IsError)
            {
                CatalogState = ContentState.Error;
                return;
            }

            SpecialItem = response.Data.Items.FirstOrDefault(c => c.Source == AudioCatalogItemSource.RecomsRecoms);
            NewAudiosItem = response.Data.Items.FirstOrDefault(c => c.Source == AudioCatalogItemSource.RecomsNewAudios);

            TryLoadBackground(SpecialItem?.Audios.FirstOrDefault());

            CatalogState = ContentState.Normal;
        }

        private async void OnPlaySpecialCommand(Audio track)
        {
            await _playerService.PlayNewTracks(SpecialItem.Audios.Select(a => a.ToPlayerTrack()), 
                track == null ? 0 : SpecialItem.Audios.IndexOf(track));

#if !WINDOWS_UWP
            _navigationService.Navigate("PlayerView", null);
#endif
        }

        private async void OnPlayNewAudiosCommand(Audio track)
        {
            await _playerService.PlayNewTracks(NewAudiosItem.Audios.Select(a => a.ToPlayerTrack()),
                track == null ? 0 : NewAudiosItem.Audios.IndexOf(track));

#if !WINDOWS_UWP
            _navigationService.Navigate("PlayerView", null);
#endif
        }

        private async void TryLoadBackground(Audio specialTrack)
        {
            if (_backgroundLoaded)
                return;
            else if (specialTrack == null)
            {
                HubBackgroundImage = HUB_BACKGROUND_DEFAULT;
                return;
            }

            _backgroundLoaded = true;

            string imagePath = await _imagesCacheService.GetCachedArtistImage(specialTrack.Artist);
            if (imagePath == null)
            {
                HubBackgroundImage = HUB_BACKGROUND_DEFAULT;
                imagePath = await _imagesCacheService.CacheAndGetArtistImage(specialTrack.Artist);
            }

            if (imagePath != null)
                HubBackgroundImage = imagePath;
            else
                _backgroundLoaded = false;
        }

        private async void OnReloadCatalogCommand() => await LoadCatalogAsync();
        private bool CanAddToMyAudios(Audio audio) => audio?.OwnerId != _inTouch.Session.UserId;

        private void OnShowTrackInfoCommand(Audio track)
        {
            var info = new VKAudioInfo(track.Id, track.OwnerId, track.Title, track.Artist, track.Url, track.Duration, track.Album?.Thumb);
            string parameter = JsonConvert.SerializeObject(info);

            if (_purchaseService.IsFullVersionPurchased)
                _navigationService.Navigate("VKAudioInfoView", parameter);
            else
                _navigationService.Navigate("PurchaseView", JsonConvert.SerializeObject(
                    new KeyValuePair<string, string>("VKAudioInfoView", parameter)));
        }

        private async void OnAddToMyAudiosCommand(Audio track)
        {
            _appLoaderService.Show(String.Format(_locService["AppLoader_AddingItem"], track.Title));

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

        private async Task<bool> AddToMyAudios(Audio audio)
        {
            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.Add(
                audio.Id, audio.OwnerId));

            if (response.IsCaptchaError())
                throw new Exception("Captcha error: cancel");
            return !response.IsError;
        }

        private bool _backgroundLoaded;

        private readonly IDialogsService _dialogsService;
        private readonly ILocService _locService;
        private readonly IAppLoaderService _appLoaderService;
    }
}
