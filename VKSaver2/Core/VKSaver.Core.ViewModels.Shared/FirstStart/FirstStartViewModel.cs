#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#elif ANDROID
using VKSaver.Core.Toolkit.Commands;
using VKSaver.Core.Services.Interfaces;
#endif

using VKSaver.Core.Toolkit;

namespace VKSaver.Core.ViewModels
{
    public sealed class FirstStartViewModel : VKSaverViewModel
    {
        public FirstStartViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            OpenFirstInternetSettings = new DelegateCommand(OnOpenFirstInternetSettings);
        }

        public DelegateCommand OpenFirstInternetSettings { get; private set; }

        private void OnOpenFirstInternetSettings()
        {
            _navigationService.Navigate("FirstInternetSettingsView", null);
            _navigationService.ClearHistory();
        }

        private readonly INavigationService _navigationService;
    }
}
