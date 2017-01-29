using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Newtonsoft.Json;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class VKSaverNavigationService : INavigationService
    {
        public VKSaverNavigationService(
            INavigationService prismNavigationService,
            IVKLoginService vkLoginService,
            IMetricaService metricaService,
            IDispatcherWrapper dispatcherWrapper,
            IServiceResolver serviceResolver)
        {
            _prismNavigationService = prismNavigationService;
            _metricaService = metricaService;
            _dispatcherWrapper = dispatcherWrapper;
            _serviceResolver = serviceResolver;

            vkLoginService.UserLogin += vkLoginService_UserLogin;
            vkLoginService.UserLogout += vkLoginService_UserLogout;

#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#endif
        }

        public bool Navigate(string pageToken, object parameter)
        {
            bool isSuccess = _prismNavigationService.Navigate(pageToken, parameter);
            if (isSuccess)
                LogNavigation(pageToken);

            return isSuccess;
        }

        public void GoBack() => _prismNavigationService.GoBack();
        public bool CanGoBack() => _prismNavigationService.CanGoBack();
        public void GoForward() => _prismNavigationService.GoForward();
        public bool CanGoForward() => _prismNavigationService.CanGoForward();
        public void ClearHistory() => _prismNavigationService.ClearHistory();
        public void RestoreSavedNavigation() => _prismNavigationService.RestoreSavedNavigation();
        public void Suspending() => _prismNavigationService.Suspending();

        public void RemoveLastPage(string pageToken = null, object parameter = null)
            => _prismNavigationService.RemoveLastPage(pageToken, parameter);

        public void RemoveAllPages(string pageToken = null, object parameter = null)
            => _prismNavigationService.RemoveAllPages(pageToken, parameter);

        public void RemoveFirstPage(string pageToken = null, object parameter = null)
            => _prismNavigationService.RemoveFirstPage(pageToken, parameter);

        private async void LogNavigation(string pageToken)
        {
            await Task.Run(() =>
            {
                var dict = new Dictionary<string, string>(1);
                dict[MetricaConstants.NAVIGATION_PAGE] = pageToken;

                _metricaService?.LogEvent(MetricaConstants.NAVIGATION_EVENT, JsonConvert.SerializeObject(dict));
            });
        }

        private async void vkLoginService_UserLogin(IVKLoginService sender, EventArgs e)
        {
            var launchViewResolver = _serviceResolver.Resolve<ILaunchViewResolver>();
            ClearHistory();
            if (!launchViewResolver.TryOpenSpecialViews() && await launchViewResolver.EnsureDatabaseUpdated() == false)
                launchViewResolver.OpenDefaultView();
        }

        private async void vkLoginService_UserLogout(IVKLoginService sender, EventArgs e)
        {
            await _dispatcherWrapper.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Navigate(AppConstants.DEFAULT_LOGIN_VIEW, null);
                ClearHistory();
            });
        }

#if WINDOWS_PHONE_APP
        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (_prismNavigationService.CanGoBack())
            {
                _prismNavigationService.GoBack();
                e.Handled = true;
            }
        }
#endif

        private readonly INavigationService _prismNavigationService;
        private readonly IMetricaService _metricaService;
        private readonly IDispatcherWrapper _dispatcherWrapper;
        private readonly IServiceResolver _serviceResolver;
    }
}
