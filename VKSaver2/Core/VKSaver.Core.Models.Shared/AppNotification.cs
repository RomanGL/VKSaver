using System;

namespace VKSaver.Core.Models
{
    /// <summary>
    /// Представляет внутреннее уведомление в приложении.
    /// </summary>
    public sealed class AppNotification
    {
        private string imageUrl;

        /// <summary>
        /// Заголовок сообщения.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Текст сообщения.
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Ссылка на картинку.
        /// </summary>
        public string ImageUrl
        {
            get
            {
                if (String.IsNullOrEmpty(imageUrl))
                {
                    switch (Type)
                    {
                        case AppNotificationType.Error:
                            imageUrl = "ms-appx:///Assets/Popups/Error.png";
                            break;
                        case AppNotificationType.Warning:
                            imageUrl = "ms-appx:///Assets/Popups/Warning.png";
                            break;
                        case AppNotificationType.Info:
                            imageUrl = "ms-appx:///Assets/Popups/Info.png";
                            break;
                        default:
                            imageUrl = "ms-appx:///Assets/Popups/Default.png";
                            break;
                    }
                }

                return imageUrl;
            }
            set { imageUrl = value; }
        }

        /// <summary>
        /// Имя представления для перехода по нажатию на уведомление.
        /// </summary>
        public string DestinationView { get; set; }
        /// <summary>
        /// Параметр навигации, передающийся на страницу при нажатии на уведомление.
        /// </summary>
        public object NavigationParameter { get; set; }
        /// <summary>
        /// Действие для выполнения.
        /// </summary>
        public Action ActionToDo { get; set; }
        /// <summary>
        /// Тип сообщения.
        /// </summary>
        public AppNotificationType Type { get; set; }
        /// <summary>
        /// Длительность уведомления.
        /// </summary>
        public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(6000);
        /// <summary>
        /// Отключить звук для этого уведомления.
        /// </summary>
        public bool NoSound { get; set; }
        /// <summary>
        /// Отключить вибрацию для этого уведомления.
        /// </summary>
        public bool NoVibration { get; set; }
    }
}
