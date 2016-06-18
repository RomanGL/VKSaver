using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace VKSaver.Controls
{
    /// <summary>
    /// Представляет элемент списка, поддерживающий команду клика.
    /// </summary>
    public class ListViewCommandItem : ListViewItem
    {
        public ListViewCommandItem()
        {
            this.DefaultStyleKey = typeof(ListViewCommandItem);
        }

        #region ReloadCommand DependencyProperty
        /// <summary>
        /// Команда, выполняющаяся при нажатии на элемент.
        /// </summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(ReloadCommandProperty); }
            set { SetValue(ReloadCommandProperty, value); }
        }

        public static readonly DependencyProperty ReloadCommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand),
            typeof(ListViewCommandItem), new PropertyMetadata(null));
        #endregion

        #region ReloadCommandParameter Property
        /// <summary>
        /// Параметр команды нажатия на элемент.
        /// </summary>
        public object CommandParameter
        {
            get { return (object)GetValue(ReloadCommandParameterProperty); }
            set { SetValue(ReloadCommandParameterProperty, value); }
        }

        public static readonly DependencyProperty ReloadCommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object),
            typeof(ListViewCommandItem), new PropertyMetadata(null));
        #endregion

        /// <summary>
        /// Вызывается при построении шаблона элемента управления.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.Tapped += (s, e) =>
            {
                if (Command != null && Command.CanExecute(CommandParameter))
                    Command.Execute(CommandParameter);
            };
        }
        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);
            this.CapturePointer(e.Pointer);
            VisualStateManager.GoToState(this, "PointerDown", true);
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);
            VisualStateManager.GoToState(this, "PointerUp", true);
            this.ReleasePointerCapture(e.Pointer);
        }
    }
}
