using System.Globalization;

namespace VKSaver.Core.Services.Interfaces
{
    /// <summary>
    /// Представляет интерфейс провайдера региональной информации для сервиса Xbox Music.
    /// </summary>
    public interface ICultureProvider
    {
        /// <summary>
        /// Возвращает информацию о региональных параметрах.
        /// </summary>
        RegionInfo GetRegionInfo();
    }
}
