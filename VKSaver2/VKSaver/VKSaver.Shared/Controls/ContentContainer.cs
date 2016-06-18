using System;
using System.Windows.Input;
using VKSaver.Core.Models.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VKSaver.Controls
{
    /// <summary>
    /// Представляет элемент управления с поддержкой 
    /// отбражения процесса загрузки и ошибок.
    /// </summary>
    [TemplatePart(Name = ReloadButtonName, Type = typeof(Button))]
    public class ContentContainer : ContentControl
    {
        private const string ReloadButtonName = "ReloadButton";
        private const string NormalStateName = "NormalState";
        private const string LoadingStateName = "LoadingState";
        private const string NoDataStateName = "NoDataState";
        private const string ErrorStateName = "ErrorState";
        private Button reloadButton;

        /// <summary>
        /// Инициализирует новый экземпляр класса.
        /// </summary>
        public ContentContainer()
        {
            this.DefaultStyleKey = typeof(ContentContainer);
        }

        #region Свойства зависимостей

        #region ContentState DependencyProperty 
        /// <summary>
        /// Состоние контента.
        /// </summary>
        public ContentState State
        {
            get { return (ContentState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(ContentState), typeof(ContentContainer), 
            new PropertyMetadata(ContentState.None, OnStateChanged));
        
        /// <summary>
        /// Вызывается при изменении состояния вложенного контента.
        /// </summary>
        private static void OnStateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((ContentContainer)obj).UpdateVisualState();
        }
        #endregion

        #region ReloadCommand DependencyProperty
        /// <summary>
        /// Команда, выполняющаяся при нажатии на кнопку повторения при ошибке.
        /// </summary>
        public ICommand ReloadCommand
        {
            get { return (ICommand)GetValue(ReloadCommandProperty); }
            set { SetValue(ReloadCommandProperty, value); }
        }

        public static readonly DependencyProperty ReloadCommandProperty =
            DependencyProperty.Register("ReloadCommand", typeof(ICommand), 
            typeof(ContentContainer), new PropertyMetadata(null));        
        #endregion

        #region ReloadCommandParameter Property
        /// <summary>
        /// Параметр команды перезагрузки при ошибке.
        /// </summary>
        public object ReloadCommandParameter
        {
            get { return (object)GetValue(ReloadCommandParameterProperty); }
            set { SetValue(ReloadCommandParameterProperty, value); }
        }

        public static readonly DependencyProperty ReloadCommandParameterProperty =
            DependencyProperty.Register("ReloadCommandParameter", typeof(object), 
            typeof(ContentContainer), new PropertyMetadata(null));        
        #endregion

        #endregion

        #region События
        /// <summary>
        /// Происходит при нажатии на кнопку перезагрузки при ошибке.
        /// </summary>
        public event EventHandler<RoutedEventArgs> ReloadButtonClick;
        #endregion

        /// <summary>
        /// Вызывается при построении шаблона элемента управления.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateVisualState();

            reloadButton = GetTemplateChild(ReloadButtonName) as Button;
            if (reloadButton == null) return;

            reloadButton.Click += (s, e) =>
            {
                if (ReloadButtonClick != null)
                    ReloadButtonClick(this, e);

                if (ReloadCommand != null && ReloadCommand.CanExecute(ReloadCommandParameter))
                    ReloadCommand.Execute(ReloadCommandParameter);
            };
        }

        private void UpdateVisualState()
        {
            string stateName = NormalStateName;
            switch (State)
            {
                case ContentState.Loading:
                    stateName = LoadingStateName;
                    break;
                case ContentState.Error:
                    stateName = ErrorStateName;
                    break;
                case ContentState.NoData:
                    stateName = NoDataStateName;
                    break;
            }
            VisualStateManager.GoToState(this, stateName, true);
        }
    }
}
