using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VKSaver.Core.Services.Database;
using VKSaver.Core.Services.Interfaces;
using Windows.Storage;

namespace VKSaver.Core.Services
{
    public sealed class LaunchViewResolver : ILaunchViewResolver
    {
        public LaunchViewResolver(
            INavigationService navigationService,
            ISettingsService settingsService)
        {
            _navigationService = navigationService;
            _settingsService = settingsService;
        }

        public string LaunchViewName
        {
            get { return _settingsService.Get(AppConstants.DEFAULT_MAIN_VIEW_PARAMETER, AppConstants.DEFAULT_MAIN_VIEW); }
            set
            {
                if (!AvailableLaunchViews.Contains(value))
                    throw new ArgumentOutOfRangeException("Неверное имя страницы. См. AvailableLaunchViews.");

                _settingsService.Set(AppConstants.DEFAULT_MAIN_VIEW_PARAMETER, value);
            }
        }

        public List<string> AvailableLaunchViews { get { return _availableLaunchViews; } }

        public void OpenDefaultView()
        {
            string viewName = LaunchViewName;
            switch (viewName)
            {
                case "UserContentView":
                    _navigationService.Navigate(viewName, JsonConvert.SerializeObject(new KeyValuePair<string, int>("audios", 0)));
                    break;
                default:
                    _navigationService.Navigate(viewName, null);
                    break;
            }
        }

        public bool TryOpenPromoView()
        {
            if (_settingsService.Get(AppConstants.CURRENT_PROMO_INDEX_PARAMETER, AppConstants.DEFAULT_PROMO_INDEX) < AppConstants.CURRENT_PROMO_INDEX)
            {
                _navigationService.Navigate("PromoView", null);
                return true;
            }

            return false;
        }

        public bool TryOpenSpecialViews()
        {
            string currentFirstView = null;
            int currentIndex = _settingsService.Get(AppConstants.CURRENT_FIRST_START_INDEX_PARAMETER, 0);
            if (currentIndex == 1)
            {
                currentFirstView = "FirstSelectLaunchView";
                _settingsService.Set(AppConstants.CURRENT_FIRST_START_VIEW_PARAMETER, currentFirstView);
            }
            else
                currentFirstView = _settingsService.Get<string>(AppConstants.CURRENT_FIRST_START_VIEW_PARAMETER, null);

            if (currentIndex < AppConstants.CURRENT_FIRST_START_INDEX)
            {
                if (currentFirstView == null)
                    _navigationService.Navigate("FirstStartView", null);
                else
                    _navigationService.Navigate("FirstStartRetryView", null);

                return true;
            }
            else if (currentFirstView != null && currentFirstView != "Completed")
            {
                _navigationService.Navigate("FirstStartRetryView", null);
                return true;
            } 

            return false;
        }

        public async Task<bool> EnsureDatabaseUpdated()
        {
            if (_settingsService.Get(AppConstants.CURRENT_LIBRARY_INDEX_PARAMETER, 0) < AppConstants.CURRENT_LIBRARY_INDEX)
            {
                _navigationService.Navigate("UpdatingDatabaseView", null);
                return true;
            }
            else
            {
                try
                {
                    var libraryDb = await ApplicationData.Current.LocalFolder.GetFileAsync(LibraryDatabase.DATABASE_FILE_NAME);                    
                }
                catch (Exception)
                {
                    _navigationService.Navigate("UpdatingDatabaseView", null);
                    return true;
                }
            }

            return false;
        }

        private readonly ISettingsService _settingsService;
        private readonly INavigationService _navigationService;

        private readonly List<string> _availableLaunchViews = new List<string>(new string[]
        {
            AppConstants.DEFAULT_MAIN_VIEW,
            "UserContentView"
        });
    }
}
