using System;
using System.Threading.Tasks;

namespace VKSaver.Core.Services.Interfaces
{
    /// <summary>
    /// Представляет сервис вибрации телефона.
    /// </summary>
    public interface IDeviceVibrationService
    {
        /// <summary>
        /// Запустить вибрацию указанной длительности.
        /// </summary>
        /// <param name="duration">Длительность вибрации.</param>
        Task Vibrate(TimeSpan duration);

        /// <summary>
        /// Запустить сценарий вибрации.
        /// </summary>
        /// <param name="vibrationData">Массив данных вибрации в милисекундах.
        /// Длительность блока вибрации и задержка чередуется. Первый элемент длительность.</param>
        Task Vibrate(int[] vibrationData);
    }
}
