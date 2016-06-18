using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Newtonsoft.Json;
using OneTeam.SDK.LastFm.Models.Audio;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class TrackInfoViewModel : ViewModelBase
    {
        public TrackInfoViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            PlayTrackCommand = new DelegateCommand(OnPlayTrackCommand);
        }

        public LFAudioBase Track { get; private set; }

        [DoNotNotify]
        public DelegateCommand PlayTrackCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            Track = JsonConvert.DeserializeObject<LFAudioBase>(e.Parameter.ToString());
            base.OnNavigatedTo(e, viewModelState);
        }

        private void OnPlayTrackCommand()
        {
            _navigationService.Navigate("AccessDeniedView", null);
        }

        private readonly INavigationService _navigationService;
    }
}
