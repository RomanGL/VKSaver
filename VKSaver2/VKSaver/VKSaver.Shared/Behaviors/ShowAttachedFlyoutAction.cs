using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace VKSaver.Behaviors
{
    /// <summary>
    /// Отображает привязанное всплывающее меню.
    /// </summary>
    public class ShowAttachedFlyoutAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {
            var element = sender as FrameworkElement;
            if (element == null) return null;

            FlyoutBase.ShowAttachedFlyout(element);
            return element;
        }
    }
}
