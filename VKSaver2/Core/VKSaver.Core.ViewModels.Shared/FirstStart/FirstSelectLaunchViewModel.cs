using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Text;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class FirstSelectLaunchViewModel : ViewModelBase
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
