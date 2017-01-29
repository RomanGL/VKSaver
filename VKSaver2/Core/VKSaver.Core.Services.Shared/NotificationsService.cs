using System;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;
using VKSaver.Core.Services.Interfaces;
using Yandex.Metrica.Push;

namespace VKSaver.Core.Services
{
    public sealed class NotificationsService : INotificationsService
    {
        public NotificationsService(
            ISettingsService settingsService,
            IVKLoginService vkLoginService)
        {
            _settingsService = settingsService;
            _vkLoginService = vkLoginService;

            _vkLoginService.UserLogin += vkLoginService_UserLogin;
            _vkLoginService.UserLogout += vkLoginService_UserLogout;
        }

        public bool IsYandexPushActivated { get; private set; }

        public Task ActivateYandexPushAsync()
        {
            return Task.Run(() =>
            {
                if (!_settingsService.Get(AppConstants.PUSH_NOTIFICATIONS_PARAMETER, true))
                    return;
                YandexMetricaPush.Activate("***REMOVED***");
                IsYandexPushActivated = true;
            });
        }

        public async Task DeactivateYandexPushAsync()
        {
            var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            channel.Close();
        }
        
        private async void vkLoginService_UserLogin(IVKLoginService sender, EventArgs e)
        {
            await ActivateYandexPushAsync();
        }

        private async void vkLoginService_UserLogout(IVKLoginService sender, EventArgs e)
        {
            await DeactivateYandexPushAsync();
        }

        private readonly ISettingsService _settingsService;
        private readonly IVKLoginService _vkLoginService;
    }
}
