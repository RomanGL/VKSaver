#if WINDOWS_UWP
using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#elif ANDROID
using VKSaver.Core.Toolkit.Commands;
#endif

using PropertyChanged;
using System.Collections.Generic;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Toolkit;
using NavigatedToEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatedToEventArgs;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class FirstSelectLaunchViewModel : VKSaverViewModel
    {
        public FirstSelectLaunchViewModel(
            INavigationService navigationService, 
            ISettingsService settingsService,
            ILaunchViewResolver launchViewResolver)
        {
            _navigationService = navigationService;
            _settingsService = settingsService;
            _launchViewResolver = launchViewResolver;

            ContinueCommand = new DelegateCommand(OnContinueCommand);
        }

        [DoNotNotify]
        public DelegateCommand ContinueCommand { get; private set; }

        public List<string> AvailableLaunchViews { get { return _launchViewResolver.AvailableLaunchViews; } }

        [AlsoNotifyFor(nameof(AvailableLaunchViews))]
        public int SelectedLaunchViewIndex
        {
            get { return AvailableLaunchViews.IndexOf(_launchViewResolver.LaunchViewName); }
            set { _launchViewResolver.LaunchViewName = AvailableLaunchViews[value]; }
        }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            _settingsService.Set(AppConstants.CURRENT_FIRST_START_VIEW_PARAMETER, "FirstSelectLaunchView");
            base.OnNavigatedTo(e, viewModelState);
        }

        private void OnContinueCommand()
        {
            _settingsService.Set(AppConstants.CURRENT_FIRST_START_INDEX_PARAMETER, AppConstants.CURRENT_FIRST_START_INDEX);
            _settingsService.Set<string>(AppConstants.CURRENT_FIRST_START_VIEW_PARAMETER, null);

            _navigationService.ClearHistory();
            _launchViewResolver.OpenDefaultView();
        }

        private readonly INavigationService _navigationService;
        private readonly ISettingsService _settingsService;
        private readonly ILaunchViewResolver _launchViewResolver;
    }
}
