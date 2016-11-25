namespace VKSaver.Core.Models
{
    /// <summary>
    /// Тип внутреннего уведомления в приложении.
    /// </summary>
    public enum AppNotificationType : byte
    {
        /// <summary>
        /// Стандартное уведомление.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Ошибка.
        /// </summary>
        Error = 1,
        /// <summary>
        /// Предупреждение.
        /// </summary>
        Warning = 2,
        /// <summary>
        /// Информация.
        /// </summary>
        Info = 3
    }
}
