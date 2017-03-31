#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#elif ANDROID
using VKSaver.Core.Toolkit.Commands;
#endif

using Newtonsoft.Json;
using PropertyChanged;
using System.Collections.Generic;
using System.Linq;
using VKSaver.Core.LinksExtractor;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Toolkit;
using VKSaver.Core.Toolkit.Navigation;
using NavigatedToEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatedToEventArgs;
using NavigatingFromEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatingFromEventArgs;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class VideoPlayerViewModel : VKSaverViewModel
    {
        public VideoPlayerViewModel(INavigationService navigationService, ILocService locService,
            IDialogsService dialogsService, IAppLoaderService appLoaderService)
        {
            _navigationService = navigationService;
            _locService = locService;
            _dialogsService = dialogsService;
            _appLoaderService = appLoaderService;

            MediaOpenedCommand = new DelegateCommand(OnMediaOpenedCommand);
            MediaEndedCommand = new DelegateCommand(OnMediaEndedCommand);
            MediaFailedCommand = new DelegateCommand(OnMediaFailedCommand);
        }

        public IVideoLink CurrentLink { get; private set; }

        public List<IVideoLink> AvailableLinks { get; private set; }

        [DoNotNotify]
        public DelegateCommand MediaOpenedCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand MediaFailedCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand MediaEndedCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (e.Parameter == null)
            {
                _navigationService.GoBack();
                return;
            }

            _appLoaderService.Show();
            var data = JsonConvert.DeserializeObject<KeyValuePair<int, List<CommonVideoLink>>>(e.Parameter.ToString());
            AvailableLinks = data.Value.Cast<IVideoLink>().ToList();
            CurrentLink = AvailableLinks[data.Key];

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatingFrom(e, viewModelState, suspending);

            if (e.NavigationMode != NavigationMode.Back)
                e.Cancel = true;
        }

        private void OnMediaOpenedCommand()
        {
            _appLoaderService.Hide();
        }

        private void OnMediaEndedCommand()
        {
            //_navigationService.GoBack();
        }

        private void OnMediaFailedCommand()
        {
            _appLoaderService.Hide();

            _dialogsService.Show(_locService["Message_VideoPlayer_Failed_Text"],
                _locService["Message_VideoPlayer_Failed_Title"]);
        }

        private readonly INavigationService _navigationService;
        private readonly IDialogsService _dialogsService;
        private readonly ILocService _locService;
        private readonly IAppLoaderService _appLoaderService;
    }
}
