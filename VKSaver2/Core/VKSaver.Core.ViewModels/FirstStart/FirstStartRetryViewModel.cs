using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.ViewModels
{
    public sealed class FirstStartRetryViewModel : ViewModelBase
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
