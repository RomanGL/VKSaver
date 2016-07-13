using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using ModernDev.InTouch;
using Newtonsoft.Json;
using OneTeam.SDK.LastFm.Models.Audio;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Collections;
using VKSaver.Core.ViewModels.Common;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class TrackInfoViewModel : ViewModelBase
    {
        public TrackInfoViewModel(INavigationService navigationService, InTouch inTouch,
            IInTouchWrapper inTouchWrapper, IPlayerService playerService,
            IDownloadsServiceHelper downloadsServiceHelper, IAppLoaderService appLoaderService)
        {
            _navigationService = navigationService;
            _inTouch = inTouch;
            _inTouchWrapper = inTouchWrapper;
            _playerService = playerService;
            _downloadsServiceHelper = downloadsServiceHelper;
            _appLoaderService = appLoaderService;

            ShowOtherTracksCommand = new DelegateCommand(OnShowOtherTracksCommand);
            PlayTracksCommand = new DelegateCommand<Audio>(OnPlayTracksCommand);
        }

        public LFAudioBase Track { get; private set; }

        [DoNotNotify]
        public DelegateCommand ShowOtherTracksCommand { get; private set; }

        [DoNotNotify]
        public SimpleStateSupportCollection<Audio> VKTracks { get; private set; }

        [DoNotNotify]
        public DelegateCommand<Audio> PlayTracksCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            Track = JsonConvert.DeserializeObject<LFAudioBase>(e.Parameter.ToString());

            if (viewModelState.Count == 0)
            {
                VKTracks = new SimpleStateSupportCollection<Audio>(LoadVKTracks);
                VKTracks.Load();
            }
            else
            {

            }

            base.OnNavigatedTo(e, viewModelState);
        }

        private async Task<IEnumerable<Audio>> LoadVKTracks()
        {
            if (Track == null || Track.Artist == null)
                return new List<Audio>(0);

            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.Search(new AudioSearchParams
            {
                AutoComplete = true,
                Count = 10,
                Query = $"{Track.Artist.Name} - {Track.Name}"
            }));

            if (response.IsError)
                throw new Exception(response.Error.ToString());

            return response.Data.Items;
        }

        private async void OnPlayTracksCommand(Audio track)
        {
            _appLoaderService.Show();

            await _playerService.PlayNewTracks(VKTracks.ToPlayerTracks(), VKTracks.IndexOf(track));
            _navigationService.Navigate("PlayerView", null);

            _appLoaderService.Hide();
        }

        private void OnShowOtherTracksCommand()
        {
            _navigationService.Navigate("AccessDeniedView", null);
        }

        private readonly INavigationService _navigationService;
        private readonly InTouch _inTouch;
        private readonly IInTouchWrapper _inTouchWrapper;
        private readonly IPlayerService _playerService;
        private readonly IDownloadsServiceHelper _downloadsServiceHelper;
        private readonly IAppLoaderService _appLoaderService;
    }
}
