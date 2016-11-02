#if WINDOWS_UWP
using Prism.Windows.Navigation;
#else
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using ModernDev.InTouch;
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
            ILocService locService,
            IInTouchWrapper inTouchWrapper,
            InTouch inTouch)
        {
            _navigationService = navigationService;
            _settingsService = settingsService;
            _dialogsService = dialogsService;
            _locService = locService;
            _inTouchWrapper = inTouchWrapper;
            _inTouch = inTouch;
        }

        public async void ActivateAds()
        {
            try
            {
                if (_locService.CurrentLanguage != "ru")
                    return;

                if (await ShowAdAsync(JOIN_BUYON_AD_NAME, JOIN_BUYON_DELAY, Subscribe(BUYON_ID), IsSubscriber(BUYON_ID)))
                    return;
                else if (await ShowAdAsync(JOIN_NSTORE_AD_NAME, JOIN_NSTORE_DELAY, Subscribe(NSTORE_ID), IsSubscriber(NSTORE_ID)))
                    return;
            }
            catch (Exception)
            {
            }
        }

        private Task<bool> ShowAdWithDateAsync(string adName, int startupsDelay, DateTime expirationdate)
        {            
            if (DateTime.Now > expirationdate)
                return Task.FromResult(false);

            return ShowAdAsync(adName, startupsDelay, new Task<bool>(() =>
            {
                _navigationService.Navigate("AdInfoView", adName);
                return true;
            }));
        }

        private async Task<bool> ShowAdAsync(string adName, int startupsDelay, Task<bool> actionToDo, Task<bool> precondition = null)
        {
            int startNumer = _settingsService.Get(adName, 0);
            if (startNumer == -1)
                return false;
            else if (startNumer > 0 && startNumer < startupsDelay)
            {
                _settingsService.Set(adName, startNumer + 1);
                return false;
            }
            else if (startNumer >= startupsDelay)
            {
                _settingsService.Set(adName, 0);
                startNumer = 0;
            }

            if (precondition != null && await precondition)
            {
                _settingsService.Set(adName, -1);
                return false;
            }

            bool result = await _dialogsService.ShowYesNoAsync(_locService[$"Ads_Message_{adName}_Text"],
                _locService[$"Ads_Message_{adName}_Title"]);

            if (result)
            {
                if (await actionToDo)
                    _settingsService.Set(adName, -1);                
            }
            else
                _settingsService.Set(adName, startNumer + 1);

            return result;
        }

        private async Task<bool> Subscribe(int groupId)
        {
            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Groups.Join(groupId));
            if (response.IsError)
                return false;

            return response.Data;
        }

        private async Task<bool> IsSubscriber(int groupId)
        {
            try
            {
                var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Groups.IsMember(groupId));
                if (response.IsError)
                    return false;
                return response.Data.IsMemeber;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private readonly INavigationService _navigationService;
        private readonly ISettingsService _settingsService;
        private readonly IDialogsService _dialogsService;
        private readonly ILocService _locService;
        private readonly IInTouchWrapper _inTouchWrapper;
        private readonly InTouch _inTouch;

        private const int DEFAULT_STARTUPS_DELAY = 10;
        private const int JOIN_BUYON_DELAY = 12;
        private const int JOIN_NSTORE_DELAY = 15;

        private const string BUYON_LUMIA_650_AD_NAME = "BuyonLumia650";
        private const string JOIN_BUYON_AD_NAME = "JoinBuyon";
        private const string JOIN_NSTORE_AD_NAME = "JoinNStore";

        private const int NSTORE_ID = 55671937;
        private const int BUYON_ID = 50073493;

        private static readonly DateTime BuyonLumia650AdDate = new DateTime(2016, 10, 24);
    }
}
