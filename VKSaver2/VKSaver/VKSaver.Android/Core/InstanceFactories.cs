using Android.Content;
using IF.Lastfm.Core.Api;
using Microsoft.Practices.Unity;
using ModernDev.InTouch;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core
{
    internal static class InstanceFactories
    {
        public static ILastAuth ResolveLastAuth(IUnityContainer container)
        {
            return new LastAuth(AppConstants.LAST_FM_API_KEY, AppConstants.LAST_FM_API_SECRET);
        }

        public static LastfmClient ResolveLastfmClient(IUnityContainer container)
        {
            var lastAuth = container.Resolve<ILastAuth>();
            return new LastfmClient((LastAuth)lastAuth);
        }

        public static ISettingsService ResolveSettingsService(IUnityContainer container)
        {
            var context = container.Resolve<Context>();
            var settingsService = new SettingsService(context);

            int settingsVersion = settingsService.Get(AppConstants.SETTINGS_VERSION_PARAMETER, 0);
            if (settingsVersion < AppConstants.SETTINGS_VERSION)
            {
                settingsService.Clear();
                settingsService.Set(AppConstants.SETTINGS_VERSION_PARAMETER, AppConstants.SETTINGS_VERSION);
            }

            if (settingsService.ContainsSetting(AppConstants.DEPRECATED_PUSH_NOTIFICATIONS_PARAMETER))
            {
                bool isEnabled = settingsService.Get(AppConstants.DEPRECATED_PUSH_NOTIFICATIONS_PARAMETER, true);
                settingsService.Remove(AppConstants.DEPRECATED_PUSH_NOTIFICATIONS_PARAMETER);
                settingsService.Set(AppConstants.PUSH_NOTIFICATIONS_PARAMETER, isEnabled);
            }

            return settingsService;
        }

        public static IVKLoginService ResolveVKLoginService(IUnityContainer container)
        {
            var settingsService = container.Resolve<ISettingsService>();
            var inTouch = container.Resolve<InTouch>();

            return new VKLoginService(settingsService, inTouch);
        }

        public static InTouch ResolveInTouch(IUnityContainer container)
        {
            return new InTouch();
        }

        public static INavigationService ResolveNavigationService(IUnityContainer container)
        {
            return null;
        }
    }
}
