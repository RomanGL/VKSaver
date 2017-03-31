#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#elif ANDROID
#endif

using System;
using System.Collections.Generic;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Toolkit;
using NavigatedToEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatedToEventArgs;
using NavigatingFromEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatingFromEventArgs;

namespace VKSaver.Core.ViewModels
{
    public sealed class PackagingFilesViewModel : VKSaverViewModel
    {
        public PackagingFilesViewModel(IDownloadsService downloadsService, 
            INavigationService navigationService, IVKLoginService vkLoginService,
            IDispatcherWrapper dispatcherWrapper)
        {
            _downloadsService = downloadsService;
            _navigationService = navigationService;
            _vkLoginService = vkLoginService;
            _dispatcherWrapper = dispatcherWrapper;
        }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);

            _downloadsService.DownloadsCompleted += DownloadsService_DownloadsCompleted;

            if (_downloadsService.GetDownloadsCount() == 0)
                CompletedNavigate();
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatingFrom(e, viewModelState, suspending);
            _downloadsService.DownloadsCompleted -= DownloadsService_DownloadsCompleted;
        }

        private void DownloadsService_DownloadsCompleted(object sender, EventArgs e)
        {
            CompletedNavigate();
        }

        private async void CompletedNavigate()
        {
            await _dispatcherWrapper.RunOnUIThreadAsync(() =>
           {
               _navigationService.ClearHistory();

               if (_vkLoginService.IsAuthorized)
                   _navigationService.Navigate("MainView", null);
               else
                   _navigationService.Navigate("LoginView", null);
           });
        }

        private readonly IDownloadsService _downloadsService;
        private readonly INavigationService _navigationService;
        private readonly IVKLoginService _vkLoginService;
        private readonly IDispatcherWrapper _dispatcherWrapper;
    }
}
