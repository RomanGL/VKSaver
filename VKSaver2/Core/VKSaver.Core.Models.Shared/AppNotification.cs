using System;
using System.ComponentModel;

namespace VKSaver.Core.Models
{
    /// <summary>
    /// Представляет внутреннее уведомление в приложении.
    /// </summary>
    public sealed class AppNotification : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler HideRequested;

        public bool IsHided { get; private set; }

        /// <summary>
        /// Заголовок сообщения.
        /// </summary>
        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title)
                    return;

                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        /// <summary>
        /// Текст сообщения.
        /// </summary>
        public string Content
        {
            get { return _content; }
            set
            {
                if (value == _content)
                    return;

                _content = value;
                OnPropertyChanged(nameof(Content));
            }
        }

        /// <summary>
        /// Процент выполнения операции (от 0 до 100).
        /// </summary>
        public double ProgressPercent
        {
            get { return _progressPercent; }
            set
            {
                if (value == _progressPercent)
                    return;

                _progressPercent = value;
                OnPropertyChanged(nameof(ProgressPercent));
            }
        }

        /// <summary>
        /// Ссылка на картинку.
        /// </summary>
        public string ImageUrl
        {
            get
            {
                if (String.IsNullOrEmpty(_imageUrl))
                {
                    switch (Type)
                    {
                        case AppNotificationType.Error:
                            _imageUrl = "ms-appx:///Assets/Popups/Error.png";
                            break;
                        case AppNotificationType.Warning:
                            _imageUrl = "ms-appx:///Assets/Popups/Warning.png";
                            break;
                        case AppNotificationType.Info:
                            _imageUrl = "ms-appx:///Assets/Popups/Info.png";
                            break;
                        default:
                            _imageUrl = "ms-appx:///Assets/Popups/Default.png";
                            break;
                    }
                }

                return _imageUrl;
            }
            set
            {
                _imageUrl = value;
                OnPropertyChanged(nameof(ImageUrl));
            }
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
        /// <summary>
        /// Указывает на важность этого уведомления. 
        /// Отображается даже при отключенных внутренних уведомлениях.
        /// </summary>
        public bool IsImportant { get; set; }

        public void Hide()
        {
            IsHided = true;
            HideRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _title;
        private string _content;
        private string _imageUrl;
        private double _progressPercent;
    }
}
