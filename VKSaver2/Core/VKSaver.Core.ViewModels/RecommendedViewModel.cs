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
    public sealed class RecommendedViewModel : ViewModelBase
    {
        public RecommendedViewModel(IVKService vkService, INavigationService navigationService,
            IPlayerService playerService)
        {
            _vkService = vkService;
            _navigationService = navigationService;
            _playerService = playerService;

            PlayTracksCommand = new DelegateCommand<VKAudio>(OnPlayTracksCommand);
        }

        public PaginatedCollection<VKAudio> Audios { get; private set; }

        [DoNotNotify]
        public DelegateCommand<VKAudio> PlayTracksCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (e.Parameter != null)
                _userID = long.Parse(e.Parameter.ToString());

            if (viewModelState.Count > 0)
            {
                Audios = JsonConvert.DeserializeObject<PaginatedCollection<VKAudio>>
                    (viewModelState[nameof(Audios)].ToString());

                Audios.LoadMoreItems = LoadMoreAudios;
            }
            else
            {
                Audios = new PaginatedCollection<VKAudio>(LoadMoreAudios);

                _audiosOffset = 0;
            }

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                viewModelState[nameof(Audios)] = JsonConvert.SerializeObject(Audios.ToList());
                viewModelState[nameof(_audiosOffset)] = _audiosOffset;
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async Task<IEnumerable<VKAudio>> LoadMoreAudios(uint page)
        {
            var parameters = new Dictionary<string, string>
            {
                { "count", "50" },
                { "offset", _audiosOffset.ToString() }
            };

            if (_userID > 0)
                parameters["user_id"] = _userID.ToString();

            var request = new Request<VKCountedItemsObject<VKAudio>>("audio.getRecommendations", parameters);
            var response = await _vkService.ExecuteRequestAsync(request);

            if (response.IsSuccess)
            {
                _audiosOffset += 50;
                return response.Response.Items;
            }
            else
                throw new Exception(response.Error.ToString());
        }

        private async void OnPlayTracksCommand(VKAudio track)
        {
            await _playerService.PlayNewTracks(Audios.Select(t => t.ToPlayerTrack()),
                Audios.IndexOf(track));
            _navigationService.Navigate("PlayerView", null);
        }

        private uint _audiosOffset;
        private long _userID;

        private readonly IVKService _vkService;
        private readonly INavigationService _navigationService;
        private readonly IPlayerService _playerService;
    }
}
