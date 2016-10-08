using System.Windows.Input;
using Windows.UI.Xaml;

namespace VKSaver.Behaviors
{
    public abstract class WithCommandDependencyObject : DependencyObject
    {
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand),
                typeof(WithCommandDependencyObject), new PropertyMetadata(null));

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object),
                typeof(WithCommandDependencyObject), new PropertyMetadata(null));
    }
}
