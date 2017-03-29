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

            vkLoginService.UserLogin += vkLoginService_UserLogin;
            vkLoginService.UserLogout += vkLoginService_UserLogout;
        }

        public bool IsYandexPushActivated { get; private set; }

        public Task ActivateYandexPushAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    if (!_settingsService.Get(AppConstants.PUSH_NOTIFICATIONS_PARAMETER, true))
                        return;
                    YandexMetricaPush.Activate("***REMOVED***");
                    IsYandexPushActivated = true;
                }
                catch (Exception)
                {
                }
            });
        }

        public async Task DeactivateYandexPushAsync()
        {
            try
            {
                IsYandexPushActivated = false;
                var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                channel.Close();
            }
            catch (Exception)
            {
            }
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
    }
}
