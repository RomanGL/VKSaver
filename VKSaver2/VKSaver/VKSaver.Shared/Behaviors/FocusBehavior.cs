using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VKSaver.Behaviors
{
    public sealed class FocusBehavior : DependencyObject, IBehavior
    {
        public DependencyObject AssociatedObject { get; private set; }

        public bool IsFocused
        {
            get { return (bool)GetValue(IsFocusedProperty); }
            set { SetValue(IsFocusedProperty, value); }
        }
        
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.Register("IsFocused", typeof(bool), 
                typeof(FocusBehavior), new PropertyMetadata(false, OnIsFocusedChanged));

        private Control AttachedControl { get { return AssociatedObject as Control; } }

        public void Attach(DependencyObject associatedObject)
        {
            AssociatedObject = associatedObject;
        }

        public void Detach()
        {
            AssociatedObject = null;
        }

        private static void OnIsFocusedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (FocusBehavior)obj;
            bool success = false;

            if ((bool)e.NewValue)
                success = behavior.AttachedControl.Focus(FocusState.Programmatic);
            else
                success = behavior.AttachedControl.Focus(FocusState.Unfocused);
        }
    }
}
