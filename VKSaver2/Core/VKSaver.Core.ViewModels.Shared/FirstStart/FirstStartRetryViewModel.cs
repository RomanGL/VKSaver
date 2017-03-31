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

using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Toolkit;

namespace VKSaver.Core.ViewModels
{
    public sealed class FirstStartRetryViewModel : VKSaverViewModel
    {
        public FirstStartRetryViewModel(INavigationService navigationService, ISettingsService settingsService)
        {
            _navigationService = navigationService;
            _settingsService = settingsService;

            ContinueCommand = new DelegateCommand(OnContinueCommand);
        }

        public DelegateCommand ContinueCommand { get; private set; }

        public void OnContinueCommand()
        {
            string lastPage = _settingsService.Get<string>(AppConstants.CURRENT_FIRST_START_VIEW_PARAMETER);
            _navigationService.Navigate(lastPage, null);
            _navigationService.ClearHistory();
        }

        private readonly INavigationService _navigationService;
        private readonly ISettingsService _settingsService;
    }
}
