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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Collections;
using VKSaver.Core.ViewModels.Common;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class AudioAlbumViewModel : ViewModelBase
    {
        public AudioAlbumViewModel(INavigationService navigationService, IPlayerService playerService,
            IDownloadsServiceHelper downloadsServiceHelper, IVKService vkService)
        {
            _navigationService = navigationService;
            _playerService = playerService;
            _downloadsServiceHelper = downloadsServiceHelper;
            _vkService = vkService;

            PlayTracksCommand = new DelegateCommand<VKAudio>(OnPlayTracksCommand);
            DownloadTrackCommand = new DelegateCommand<VKAudio>(OnDownloadTrackCommand);
        }

        public PaginatedCollection<VKAudio> Tracks { get; private set; }

        [DoNotNotify]
        public DelegateCommand<VKAudio> PlayTracksCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<VKAudio> DownloadTrackCommand { get; private set; }

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

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                viewModelState[nameof(Tracks)] = JsonConvert.SerializeObject(Tracks.ToList());
                viewModelState[nameof(_offset)] = _offset;
            }

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
                _offset += 50;
                return response.Response.Items;
            }
            else
                throw new Exception(response.Error.ToString());
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

        private uint _offset;

        private readonly INavigationService _navigationService;
        private readonly IPlayerService _playerService;
        private readonly IDownloadsServiceHelper _downloadsServiceHelper;
        private readonly IVKService _vkService;
    }
}
