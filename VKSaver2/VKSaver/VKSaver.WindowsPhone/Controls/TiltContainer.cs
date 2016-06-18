using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace VKSaver.Controls
{
    /// <summary>
    /// Представляет контейнер для одного элемента, поддерживающий эффект нажатия.
    /// </summary>
    public sealed class TiltContainer : ContentControl
    {
        public TiltContainer()
        {
            this.DefaultStyleKey = typeof(TiltContainer);
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
