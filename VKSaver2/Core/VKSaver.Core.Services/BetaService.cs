using Microsoft.Practices.Prism.StoreApps.Interfaces;
using ModernDev.InTouch;
using System;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class BetaService : IBetaService
    {
        public BetaService(InTouch inTouch, INavigationService navigationService)
        {
            _inTouch = inTouch;
            _navigationService = navigationService;
        }

        public async void ExecuteAppLaunch()
        {            
            try
            {
                await Task.Delay(1000);
                var response = await _inTouch.Request<bool>("execute.isBetaAvailable");
                if (!response.IsError && !response.Data)
                {
                    _navigationService.Navigate("BetaBlockerView", null);
                    _navigationService.ClearHistory();
                }
            }
            catch { }
        }

        private readonly InTouch _inTouch;
        private readonly INavigationService _navigationService;
    }
}
