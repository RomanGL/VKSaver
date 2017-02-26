using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace VKSaver.Controls
{
    public sealed class HamburgerButton : ButtonBase
    {
        private const string NormalState = "Normal";
        private const string PointerOverState = "PointerOver";
        private const string PressedState = "Pressed";

        public HamburgerButton()
        {
            this.DefaultStyleKey = typeof(HamburgerButton);
        }

        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            base.OnPointerMoved(e);
            VisualStateManager.GoToState(this, PointerOverState, true);
        }

        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);
            VisualStateManager.GoToState(this, PointerOverState, true);
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
            VisualStateManager.GoToState(this, NormalState, true);
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);
            VisualStateManager.GoToState(this, PressedState, true);
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);
            VisualStateManager.GoToState(this, NormalState, true);
        }
    }
}
