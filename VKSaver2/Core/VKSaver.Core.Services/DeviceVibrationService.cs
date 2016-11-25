using System;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using Windows.Phone.Devices.Notification;

namespace VKSaver.Core.Services
{
    /// <summary>
    /// Представляет сервис вибрации телефона.
    /// </summary>
    public sealed class DeviceVibrationService : IDeviceVibrationService
    {
        private readonly VibrationDevice device;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DeviceVibrationService"/>.
        /// </summary>
        public DeviceVibrationService()
        {
            device = VibrationDevice.GetDefault();
        }

        /// <summary>
        /// Запустить вибрацию указанной длительности.
        /// </summary>
        /// <param name="duration">Длительность вибрации.</param>
        public Task Vibrate(TimeSpan duration)
        {
            return Task.Run(() => device.Vibrate(duration));
        }

        /// <summary>
        /// Запустить сценарий вибрации.
        /// </summary>
        /// <param name="vibrationData">Массив данных вибрации в милисекундах.
        /// Длительность блока вибрации и задержка чередуется. Первый элемент длительность.</param>
        public async Task Vibrate(int[] vibrationData)
        {
            for (int i = 0; i < vibrationData.Length; i++)
            {
                if (i % 2 == 0) device.Vibrate(TimeSpan.FromMilliseconds(vibrationData[i]));
                else await Task.Delay(vibrationData[i]);
            }
        }
    }
}
