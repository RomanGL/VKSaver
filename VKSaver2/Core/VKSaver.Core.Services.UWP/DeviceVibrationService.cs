using System;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    /// <summary>
    /// Представляет сервис вибрации телефона.
    /// </summary>
    public sealed class DeviceVibrationService : IDeviceVibrationService
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DeviceVibrationService"/>.
        /// </summary>
        public DeviceVibrationService()
        {
        }

        /// <summary>
        /// Запустить вибрацию указанной длительности.
        /// </summary>
        /// <param name="duration">Длительность вибрации.</param>
        public Task Vibrate(TimeSpan duration)
        {
            // TODO: Вибрация.
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Запустить сценарий вибрации.
        /// </summary>
        /// <param name="vibrationData">Массив данных вибрации в милисекундах.
        /// Длительность блока вибрации и задержка чередуется. Первый элемент длительность.</param>
        public Task Vibrate(int[] vibrationData)
        {
            // TODO: Вибрация.
            return Task.FromResult<object>(null);
        }
    }
}
