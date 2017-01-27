using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

// Документацию по шаблону элемента "Элемент управления на основе шаблона" см. по адресу http://go.microsoft.com/fwlink/?LinkId=234235

namespace VKSaver.Controls
{
    public sealed class DoubleTapLikeControl : Control
    {
        public DoubleTapLikeControl()
        {
            this.DefaultStyleKey = typeof(DoubleTapLikeControl);
        }
        
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(DoubleTapLikeControl), new PropertyMetadata(null));
        
        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
        
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(DoubleTapLikeControl), new PropertyMetadata(null));

        public void PlayLikeAnimation()
        {
            _likeStoryboard.Begin();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _likeStoryboard = GetTemplateChild("LikeStoryboard") as Storyboard;
            this.DoubleTapped += DoubleTapLikeControl_DoubleTapped;
        }

        private void DoubleTapLikeControl_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            PlayLikeAnimation();
            if (Command != null && Command.CanExecute(CommandParameter))
                Command.Execute(CommandParameter);
        }

        private Storyboard _likeStoryboard;
    }
}
