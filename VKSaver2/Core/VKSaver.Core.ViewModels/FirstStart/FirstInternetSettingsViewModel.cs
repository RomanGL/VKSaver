using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using System.Collections.Generic;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.ViewModels
{
    public sealed class FirstInternetSettingsViewModel : ViewModelBase
    {
        public FirstInternetSettingsViewModel(INavigationService navigationService, ISettingsService settingsService)
        {
            _navigationService = navigationService;
            _settingsService = settingsService;

            SelectInternetParameter = new DelegateCommand<string>(OnSelectInternetParameter);
        }

        public DelegateCommand<string> SelectInternetParameter { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            _settingsService.Set(AppConstants.CURRENT_FIRST_START_VIEW_PARAMETER, "FirstInternetSettingsView");
            base.OnNavigatedTo(e, viewModelState);
        }

        private void OnSelectInternetParameter(string parameter)
        {
            switch (parameter)
            {
                case "yes":
                    _settingsService.Set(AppConstants.INTERNET_ACCESS_TYPE, InternetAccessType.All);
                    break;
                case "wifi":
                    _settingsService.Set(AppConstants.INTERNET_ACCESS_TYPE, InternetAccessType.WiFi);
                    break;
                case "no":
                    _settingsService.Set(AppConstants.INTERNET_ACCESS_TYPE, InternetAccessType.Disabled);
                    break;
            }

            _navigationService.Navigate("UpdatingDatabaseView", true);
            _navigationService.ClearHistory();
        }

        private readonly INavigationService _navigationService;
        private readonly ISettingsService _settingsService;
    }
}
