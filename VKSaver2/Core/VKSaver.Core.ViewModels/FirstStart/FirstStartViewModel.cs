using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;

namespace VKSaver.Core.ViewModels
{
    public sealed class FirstStartViewModel : ViewModelBase
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
