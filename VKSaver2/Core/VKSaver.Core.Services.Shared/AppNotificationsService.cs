using System;
using System.Threading.Tasks;
using VKSaver.Core.Models;
using VKSaver.Core.Services.Interfaces;
using static VKSaver.Core.Services.AppConstants;

namespace VKSaver.Core.Services
{
    /// <summary>
    /// Представляет сервис для отправки уведомлений внутри приложения.
    /// </summary>
    public class AppNotificationsService : IAppNotificationsService
    {
        /// <summary>
        /// Происходит при получении нового уведомления для отображения.
        /// </summary>
        public event TypedEventHandler<IAppNotificationsService, AppNotification> NewNotification;

        /// <summary>
        /// Инициализирует ноый экземпляр класса <see cref="AppNotificationsService"/>.
        /// </summary>
        /// <param name="presenter">Презентер уведомлений.</param>
        /// <param name="soundService">Сервис воспроизведения звуковых эффектов.</param>
        /// <param name="deviceVibrationService">Сервис вибрации.</param>
        /// <param name="settingsService">Сервис настроек приложения.</param>
        /// <param name="locService">Сервис локализаций.</param>
        public AppNotificationsService(
            IAppNotificationsPresenter presenter, 
            ISoundService soundService,
            IDeviceVibrationService deviceVibrationService, 
            ISettingsService settingsService,
            ILocService locService)
        {
            _presenter = presenter;
            _soundService = soundService;
            _deviceVibrationService = deviceVibrationService;
            _settingsService = settingsService;
            _locService = locService;
        }

        /// <summary>
        /// Отправить уведомление.
        /// </summary>
        /// <param name="notification">Данные уведомления.</param>
        public void SendNotification(AppNotification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            if (!_settingsService.GetNoCache(ENABLE_IN_APP_POPUPS, true) && !notification.IsImportant)
                return;

            Task.Run(async () =>
            {
                try
                {
                    if (!notification.NoSound && _settingsService.GetNoCache(ENABLE_IN_APP_SOUND, true))
                        await PlaySound(notification.Type);
                }
                catch (Exception)
                {
                    _settingsService.Set(ENABLE_IN_APP_SOUND, false);
                    var error = new AppNotification
                    {
                        Type = AppNotificationType.Error,
                        NoSound = true,
                        Title = _locService["AppNotifications_SoundNotWorking_Title"],
                        Content = _locService["AppNotifications_SoundNotWorking_Content"],
                        IsImportant = true
                    };
                    SendNotification(error);
                }
            });

            _presenter.ShowNotification(notification);

            Task.Run(async () =>
            {
                try
                {
                    if (!notification.NoVibration && _settingsService.GetNoCache(ENABLE_IN_APP_VIBRATION, true))
                        await StartVibration(notification.Type);
                }
                catch (Exception)
                {
                    _settingsService.Set(ENABLE_IN_APP_VIBRATION, false);
                    var error = new AppNotification
                    {
                        Type = AppNotificationType.Error,
                        NoVibration = true,
                        Title = _locService["AppNotifications_VibrationNotWorking_Title"],
                        Content = _locService["AppNotifications_VibrationNotWorking_Content"],
                        IsImportant = true
                    };
                    SendNotification(error);
                }
            });

            NewNotification?.Invoke(this, notification);
        }

        /// <summary>
        /// Воспроизвести звук уведомления.
        /// </summary>
        /// <param name="type">Тип уведомления.</param>
        private async Task PlaySound(AppNotificationType type)
        {
            switch (type)
            {
                case AppNotificationType.Error:
                    await _soundService.PlaySound(ERROR_NOTIFICATION_SOUND_SOURCE);
                    break;
                case AppNotificationType.Warning:
                    await _soundService.PlaySound(WARNING_NOTIFICATION_SOUND_SOURCE);
                    break;
                case AppNotificationType.Info:
                    await _soundService.PlaySound(INFO_NOTIFICATION_SOUND_SOURCE);
                    break;
                default:
                    await _soundService.PlaySound(DEFAULT_NOTIFICATION_SOUND_SOURCE);
                    break;
            }
        }

        /// <summary>
        /// Запустить вибрацию уведомления.
        /// </summary>
        /// <param name="type">Тип уведомления.</param>
        private async Task StartVibration(AppNotificationType type)
        {
            switch (type)
            {
                case AppNotificationType.Error:
                    await _deviceVibrationService.Vibrate(ErrorNotificationVibrationData);
                    break;
                case AppNotificationType.Warning:
                    await _deviceVibrationService.Vibrate(WarningNotificationVibrationData);
                    break;
                case AppNotificationType.Info:
                    await _deviceVibrationService.Vibrate(InfoVibrationDuration);
                    break;
                default:
                    await _deviceVibrationService.Vibrate(DefaultNotificationVibrationData);
                    break;
            }
        }

        private readonly IAppNotificationsPresenter _presenter;
        private readonly ISoundService _soundService;
        private readonly IDeviceVibrationService _deviceVibrationService;
        private readonly ISettingsService _settingsService;
        private readonly ILocService _locService;

        private static readonly TimeSpan InfoVibrationDuration = TimeSpan.FromMilliseconds(50);
        private static readonly int[] DefaultNotificationVibrationData = new int[]
            { 50, 150, 50, 150, 50 };
        private static readonly int[] WarningNotificationVibrationData = new int[]
            { 300, 400, 150 };
        private static readonly int[] ErrorNotificationVibrationData = new int[]
            { 50, 150, 50, 150, 50, 150, 100 };

        private const string DEFAULT_NOTIFICATION_SOUND_SOURCE = "ms-appx:///Assets/Sounds/Default.wav";
        private const string WARNING_NOTIFICATION_SOUND_SOURCE = "ms-appx:///Assets/Sounds/Warning.wav";
        private const string ERROR_NOTIFICATION_SOUND_SOURCE = "ms-appx:///Assets/Sounds/Error.wav";
        private const string INFO_NOTIFICATION_SOUND_SOURCE = "ms-appx:///Assets/Sounds/Info.wav";
    }
}
