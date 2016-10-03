using ModernDev.InTouch;
using System;
using System.Globalization;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Interfaces;
using Windows.ApplicationModel.Resources;
using Windows.Globalization;
using VKSaver.Core.Models.Extensions;
using System.Collections.Generic;
using Windows.System.UserProfile;
using Windows.ApplicationModel.Resources.Core;

namespace VKSaver.Core.Services
{
    public sealed class LocService : ILocService
    {
        public LocService(ISettingsService settingsService, InTouch inTouch)
        {
            _loader = ResourceLoader.GetForViewIndependentUse();
            _settingsService = settingsService;
            _inTouch = inTouch;
        }

        public string this[string resName] { get { return GetResource(resName); } }

        public AppLanguage CurrentAppLanguage
        {
            get { return _settingsService.Get(APP_LANGUAGE_PARAMETER, AppLanguage.AsSystem); }
            set
            {
                _settingsService.Set(APP_LANGUAGE_PARAMETER, value);
                //ApplyAppLanguageInternal(true);
            }
        }

        public string CurrentLanguage
        {
            get
            {
                string lang = null;

                var appLang = CurrentAppLanguage;
                if (appLang == AppLanguage.AsSystem)
                    lang = GlobalizationPreferences.Languages[0].Split(new char[] { '-' })[0];
                else
                    lang = appLang.ToLangString();

                return lang;
            }
        }

        public string GetResource(string resName)
        {
            return _loader.GetString(resName);
        }

        public List<LanguageItem> GetAvailableLanguages()
        {
            return new List<LanguageItem>
            {
                new LanguageItem { Lang = AppLanguage.AsSystem, Name = this["Language_SystemLanguage_Text"] },
                new LanguageItem { Lang = AppLanguage.Russian, Name = new CultureInfo("ru").DisplayName },
                new LanguageItem { Lang = AppLanguage.Ukrainian, Name = new CultureInfo("uk").DisplayName },
                new LanguageItem { Lang = AppLanguage.Belorussian, Name = new CultureInfo("be").DisplayName },
                new LanguageItem { Lang = AppLanguage.English, Name = new CultureInfo("en").DisplayName },
                new LanguageItem { Lang = AppLanguage.German, Name = new CultureInfo("de").DisplayName }
            };
        }

        public void ApplyAppLanguage()
        {
            ApplyAppLanguageInternal(false);
        }

        private void ApplyAppLanguageInternal(bool isInRuntime)
        {
            var currentLang = CurrentAppLanguage;

            string lang = "";
            if (currentLang != AppLanguage.AsSystem)
                lang = currentLang.ToLangString();

            ApplicationLanguages.PrimaryLanguageOverride = lang;

            if (lang != "")
            {
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(lang);
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(lang);
            }

            _inTouch.DataLanguage = CurrentLanguage.ToInTouchLang();

            if (isInRuntime)
            {
                ResourceContext.GetForViewIndependentUse().Reset();
                ResourceContext.GetForCurrentView().Reset();
                _loader = ResourceLoader.GetForViewIndependentUse();
            }
        }
        
        private ResourceLoader _loader;

        private readonly ISettingsService _settingsService;
        private readonly InTouch _inTouch;

        private const string APP_LANGUAGE_PARAMETER = "AppLanguage";        
    }
}
