using System.Globalization;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    /// <summary>
    /// Представляет провайдер региональных параметров для сервиса Xbox Music.
    /// </summary>
    public class CultureProvider : ICultureProvider
    {
        /// <summary>
        /// Возвращает информацию о региональных параметрах.
        /// </summary>
        public RegionInfo GetRegionInfo()
        {
#if WINDOWS_PHONE_APP
            return new RegionInfo("en-us");
#else
            return new RegionInfo("us");
#endif
        }
    }
}
