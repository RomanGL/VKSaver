using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// Документацию по шаблону элемента "Элемент управления на основе шаблона" см. по адресу http://go.microsoft.com/fwlink/?LinkId=234235

namespace VKSaver.Controls
{
    /// <summary>
    /// Представляет элемент, отображающий индикатор работы.
    /// </summary>
    public sealed class BusyControl : Control
    {
        public BusyControl()
        {
            DefaultStyleKey = typeof(BusyControl);
        }
        
        /// <summary>
        /// Активен ли индикатор.
        /// </summary>
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(BusyControl), new PropertyMetadata(default(bool), OnIsActiveChanged));

        /// <summary>
        /// Происходит при изменении свойства активности.
        /// </summary>
        private static void OnIsActiveChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var busy = (BusyControl)obj;
            VisualStateManager.GoToState(busy, (bool)e.NewValue ? "Visible" : "Collapsed", true);
        }

        /// <summary>
        /// Вызывается при построении шаблона.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            VisualStateManager.GoToState(this, IsActive ? "Visible" : "Collapsed", true);
        }
    }
}
