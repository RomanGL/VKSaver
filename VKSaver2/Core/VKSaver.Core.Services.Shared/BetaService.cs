#if WINDOWS_UWP
using Prism.Windows.Navigation;
#else
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#endif

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
                await Task.Delay(1500);
                if (CheckBetaDate())
                {
                    _navigationService.Navigate("BetaBlockerView", null);
                    _navigationService.ClearHistory();
                }
            }
            catch { }
        }

        private bool CheckBetaDate()
        {
            return DateTime.Now >= _maxDate;
        }

        private readonly DateTime _maxDate = new DateTime(2017, 10, 20);
        private readonly InTouch _inTouch;
        private readonly INavigationService _navigationService;
    }
}
