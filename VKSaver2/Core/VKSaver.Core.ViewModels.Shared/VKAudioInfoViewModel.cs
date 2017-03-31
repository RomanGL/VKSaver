#if WINDOWS_UWP
using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
#elif ANDROID
using VKSaver.Core.Toolkit.Commands;
#endif

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PropertyChanged;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Toolkit;
using NavigatedToEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatedToEventArgs;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class VKAudioInfoViewModel : VKSaverViewModel
    {
        public VKAudioInfoViewModel(
            IHttpFileService httpFileService, 
            IImagesCacheService imagesCacheService)
        {
            _httpFileService = httpFileService;
            _imagesCacheService = imagesCacheService;

            ReloadInfoCommand = new DelegateCommand(LoadTrackInfo);
        }

        public VKAudioInfo Track { get; private set; }

        public string ArtistImage { get; private set; }

        public string TrackImage { get; private set; }

        public FileSize Size { get; private set; }

        public int Bitrate { get; private set; }

        public ContentState InfoState { get; private set; }

        [DoNotNotify]
        public DelegateCommand ReloadInfoCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (e.Parameter != null)
            {
                Track = JsonConvert.DeserializeObject<VKAudioInfo>(e.Parameter.ToString());
                LoadArtistImage();
                LoadTrackImage();
                LoadTrackInfo();
            }

            base.OnNavigatedTo(e, viewModelState);
        }

        private async void LoadArtistImage()
        {
            ArtistImage = await _imagesCacheService.GetCachedArtistImage(Track.Artist);
            if (ArtistImage == null)
            {
                ArtistImage = AppConstants.DEFAULT_PLAYER_BACKGROUND_IMAGE;
                var img = await _imagesCacheService.CacheAndGetArtistImage(Track.Artist);
                if (img != null)
                    ArtistImage = img;
            }
        }

        private async void LoadTrackImage()
        {
            TrackImage = await _imagesCacheService.GetCachedAlbumImage(Track.Title);
            if (TrackImage == null)
            {
                TrackImage = await _imagesCacheService.CacheAndGetAlbumImage(Track.Title, Track.Artist);
            }
        }

        private async void LoadTrackInfo()
        {
            InfoState = ContentState.Loading;
            try
            {
                Size = await _httpFileService.GetFileSize(Track.Source);
                Bitrate = (int)Size.Kilobytes * 8 / Track.Duration;
                InfoState = ContentState.Normal;
            }
            catch (Exception)
            {
                InfoState = ContentState.Error;
            }
        }

        private readonly IHttpFileService _httpFileService;
        private readonly IImagesCacheService _imagesCacheService;
    }
}
