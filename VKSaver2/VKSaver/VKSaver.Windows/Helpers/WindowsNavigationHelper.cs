using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GalaSoft.MvvmLight.Command;
using System;
using VKSaver.Enums.App;

namespace VKSaver.Helpers
{
    /// <summary>
    /// Представляет помощник навигации.
    /// </summary>
    public class WindowsNavigationHelper : DependencyObject
    {
        private Page Page { get; set; }
        private Frame Frame { get { return this.Page.Frame; } }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="NavigationHelper"/>.
        /// </summary>
        /// <param name="page">Ссылка на текущую страницу, используемая для навигации.  
        /// Эта ссылка позволяет осуществлять различные действия с кадрами и гарантировать, что клавиатура 
        /// запросы навигации происходят, только когда страница занимает все окно.</param>
        public WindowsNavigationHelper(Page page)
        {
            this.Page = page;

            // Если данная страница является частью визуального дерева, возникают два изменения:
            // 1) Сопоставление состояния просмотра приложения с визуальным состоянием для страницы.
            // 2) Обработка запросов на аппаратные переходы
            this.Page.Loaded += (sender, e) =>
            {
                // Навигация с помощью мыши и клавиатуры применяется, только если страница занимает все окно
                if (this.Page.ActualHeight == Window.Current.Bounds.Height &&
                    this.Page.ActualWidth == Window.Current.Bounds.Width)
                {
                    // Непосредственное прослушивание окна, поэтому фокус не требуется
                    Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated +=
                        CoreDispatcher_AcceleratorKeyActivated;
                    Window.Current.CoreWindow.PointerPressed +=
                        this.CoreWindow_PointerPressed;
                }
            };

            // Отмена тех же изменений, когда страница перестает быть видимой
            this.Page.Unloaded += (sender, e) =>
            {
                Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated -=
                    CoreDispatcher_AcceleratorKeyActivated;
                Window.Current.CoreWindow.PointerPressed -=
                    this.CoreWindow_PointerPressed;
            };
        }

        #region Поддержка навигации

        RelayCommand _goBackCommand;
        RelayCommand _goForwardCommand;


        /// <summary>
        /// <see cref="RelayCommand"/> используется для привязки к свойству Command кнопки "Назад"
        /// для перехода к последнему элементу в журнале обратных переходов, если кадр
        /// управляет собственным журналом навигации.
        /// 
        /// <see cref="RelayCommand"/> настроено на использование виртуального метода <see cref="GoBack"/>
        /// в качестве действия Execute Action и <see cref="CanGoBack"/> для CanExecute.
        /// </summary>
        public RelayCommand GoBackCommand
        {
            get
            {
                if (_goBackCommand == null)
                {
                    _goBackCommand = new RelayCommand(
                        () => this.GoBack(),
                        () => this.CanGoBack());
                }
                return _goBackCommand;
            }
            set
            {
                _goBackCommand = value;
            }
        }
        /// <summary>
        /// <see cref="RelayCommand"/> используется для перехода к самому последнему элементу в 
        /// журнал прямой навигации, если кадр самостоятельно управляет своим журналом навигации.
        /// 
        /// <see cref="RelayCommand"/> настроено на использование виртуального метода <see cref="GoForward"/>
        /// в качестве действия Execute Action и <see cref="CanGoForward"/> для CanExecute.
        /// </summary>
        public RelayCommand GoForwardCommand
        {
            get
            {
                if (_goForwardCommand == null)
                {
                    _goForwardCommand = new RelayCommand(
                        () => this.GoForward(),
                        () => this.CanGoForward());
                }
                return _goForwardCommand;
            }
        }

        /// <summary>
        /// Виртуальный метод, используемый свойством <see cref="GoBackCommand"/>
        /// для определения возможности перехода <see cref="Frame"/> назад.
        /// </summary>
        /// <returns>
        /// true, если <see cref="Frame"/> имеет хотя бы одно вхождение 
        /// в журнале обратной навигации.
        /// </returns>
        public virtual bool CanGoBack()
        {
            return this.Frame != null && this.Frame.CanGoBack;
        }
        /// <summary>
        /// Виртуальный метод, используемый свойством <see cref="GoForwardCommand"/>
        /// для определения возможности перехода <see cref="Frame"/> вперед.
        /// </summary>
        /// <returns>
        /// true, если <see cref="Frame"/> имеет хотя бы одно вхождение 
        /// в журнале прямой навигации.
        /// </returns>
        public virtual bool CanGoForward()
        {
            return this.Frame != null && this.Frame.CanGoForward;
        }

        /// <summary>
        /// Виртуальный метод, используемый свойством <see cref="GoBackCommand"/>
        /// для вызова метода <see cref="Windows.UI.Xaml.Controls.Frame.GoBack"/>.
        /// </summary>
        public virtual void GoBack()
        {
            if (this.Frame != null && this.Frame.CanGoBack) this.Frame.GoBack();
        }
        /// <summary>
        /// Виртуальный метод, используемый свойством <see cref="GoForwardCommand"/>
        /// для вызова метода <see cref="Windows.UI.Xaml.Controls.Frame.GoForward"/>.
        /// </summary>
        public virtual void GoForward()
        {
            if (this.Frame != null && this.Frame.CanGoForward) this.Frame.GoForward();
        }
        /// <summary>
        /// Вызывается при каждом нажатии клавиши, включая системные клавиши, такие как клавиша ALT, если
        /// данная страница активна и занимает все окно.  Используется для обнаружения навигации с помощью клавиатуры
        /// между страницами, даже если сама страница не имеет фокуса.
        /// </summary>
        /// <param name="sender">Экземпляр, инициировавший событие.</param>
        /// <param name="e">Данные события, описывающие условия, которые привели к возникновению события.</param>
        private void CoreDispatcher_AcceleratorKeyActivated(CoreDispatcher sender,
            AcceleratorKeyEventArgs e)
        {
            var virtualKey = e.VirtualKey;

            // Дальнейшее изучение следует выполнять, только если нажата клавиша со стрелкой влево или вправо либо назначенная клавиша "Назад" или
            // нажаты
            if ((e.EventType == CoreAcceleratorKeyEventType.SystemKeyDown ||
                e.EventType == CoreAcceleratorKeyEventType.KeyDown) &&
                (virtualKey == VirtualKey.Left || virtualKey == VirtualKey.Right ||
                (int)virtualKey == 166 || (int)virtualKey == 167))
            {
                var coreWindow = Window.Current.CoreWindow;
                var downState = CoreVirtualKeyStates.Down;
                bool menuKey = (coreWindow.GetKeyState(VirtualKey.Menu) & downState) == downState;
                bool controlKey = (coreWindow.GetKeyState(VirtualKey.Control) & downState) == downState;
                bool shiftKey = (coreWindow.GetKeyState(VirtualKey.Shift) & downState) == downState;
                bool noModifiers = !menuKey && !controlKey && !shiftKey;
                bool onlyAlt = menuKey && !controlKey && !shiftKey;

                if (((int)virtualKey == 166 && noModifiers) ||
                    (virtualKey == VirtualKey.Left && onlyAlt))
                {
                    // Переход назад при нажатии клавиши "Назад" или сочетания клавиш ALT+СТРЕЛКА ВЛЕВО
                    e.Handled = true;
                    this.GoBackCommand.Execute(null);
                }
                else if (((int)virtualKey == 167 && noModifiers) ||
                    (virtualKey == VirtualKey.Right && onlyAlt))
                {
                    // Переход вперед при нажатии клавиши "Вперед" или сочетания клавиш ALT+СТРЕЛКА ВПРАВО
                    e.Handled = true;
                    this.GoForwardCommand.Execute(null);
                }
            }
        }

        /// <summary>
        /// Вызывается при каждом щелчке мыши, касании сенсорного экрана или аналогичном действии, если эта
        /// страница активна и занимает все окно.  Используется для обнаружения нажатий мышью кнопок "Вперед" и
        /// "Назад" в браузере для перехода между страницами.
        /// </summary>
        /// <param name="sender">Экземпляр, инициировавший событие.</param>
        /// <param name="e">Данные события, описывающие условия, которые привели к возникновению события.</param>
        private void CoreWindow_PointerPressed(CoreWindow sender,
            PointerEventArgs e)
        {
            var properties = e.CurrentPoint.Properties;

            // Пропуск сочетаний кнопок, включающих левую, правую и среднюю кнопки
            if (properties.IsLeftButtonPressed || properties.IsRightButtonPressed ||
                properties.IsMiddleButtonPressed)
                return;

            // Если нажата кнопка "Назад" или "Вперед" (но не обе), выполняется соответствующий переход
            bool backPressed = properties.IsXButton1Pressed;
            bool forwardPressed = properties.IsXButton2Pressed;
            if (backPressed ^ forwardPressed)
            {
                e.Handled = true;
                if (backPressed) this.GoBackCommand.Execute(null);
                if (forwardPressed) this.GoForwardCommand.Execute(null);
            }
        }

        #endregion
    }
}
