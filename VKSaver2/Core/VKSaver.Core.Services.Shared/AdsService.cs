#if WINDOWS_UWP
using Prism.Windows.Navigation;
#else
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#endif

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class AdsService : IAdsService
    {
        public AdsService(
            INavigationService navigationService,
            ISettingsService settingsService,
            IDialogsService dialogsService,
            ILocService locService)
        {
            _navigationService = navigationService;
            _settingsService = settingsService;
            _dialogsService = dialogsService;
            _locService = locService;
        }

        public async void ActivateAds()
        {
            try
            {
                if (_locService.CurrentLanguage != "ru")
                    return;

                await ShowAdWithDateAsync(BUYON_LUMIA_650_AD_NAME, BuyonLumia650AdDate);
            }
            catch (Exception)
            {
            }
        }

        private Task<bool> ShowAdWithDateAsync(string adName, DateTime expirationdate)
        {            
            if (DateTime.Now > expirationdate)
                return Task.FromResult(false);

            return ShowAdAsync(adName);
        }

        private async Task<bool> ShowAdAsync(string adName)
        {
            int startNumer = _settingsService.Get(adName, 0);
            if (startNumer == -1)
                return false;
            else if (startNumer > 0 && startNumer < STARTUPS_DELAY)
            {
                _settingsService.Set(adName, startNumer + 1);
                return false;
            }
            else if (startNumer >= STARTUPS_DELAY)
            {
                _settingsService.Set(adName, 0);
                startNumer = 0;
            }

            bool result = await _dialogsService.ShowYesNoAsync(_locService[$"Ads_Message_{adName}_Text"],
                _locService[$"Ads_Message_{adName}_Title"]);

            if (result)
            {
                _settingsService.Set(adName, -1);
                _navigationService.Navigate("AdInfoView", adName);
            }
            else
                _settingsService.Set(adName, startNumer + 1);

            return result;
        }

        private readonly INavigationService _navigationService;
        private readonly ISettingsService _settingsService;
        private readonly IDialogsService _dialogsService;
        private readonly ILocService _locService;

        private const int STARTUPS_DELAY = 10;
        private const string BUYON_LUMIA_650_AD_NAME = "BuyonLumia650";
        private static readonly DateTime BuyonLumia650AdDate = new DateTime(2016, 10, 24);
    }
}
