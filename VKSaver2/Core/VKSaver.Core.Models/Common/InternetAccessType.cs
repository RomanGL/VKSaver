namespace VKSaver.Core.Models.Common
{
    /// <summary>
    /// Тип доступа ВКачай к интернету.
    /// </summary>
    public enum InternetAccessType : byte
    {
        /// <summary>
        /// Все сети.
        /// </summary>
        All = 0,
        /// <summary>
        /// Только через Wi-Fi.
        /// </summary>
        WiFi = 1,
        /// <summary>
        /// Доступ к интернету запрещен.
        /// </summary>
        Disabled = 2
    }
}
