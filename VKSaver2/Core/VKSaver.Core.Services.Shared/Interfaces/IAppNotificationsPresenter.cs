using VKSaver.Core.Models;

namespace VKSaver.Core.Services.Interfaces
{
    /// <summary>
    /// Представляет методы для визуального отображения внутренних уведомлений приложения.
    /// </summary>
    public interface IAppNotificationsPresenter
    {
        /// <summary>
        /// Показать уведомление на экране.
        /// </summary>
        /// <param name="notification">Данные уведомления.</param>
        void ShowNotification(AppNotification notification);
    }
}
