using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Unity;
using Prism.Windows.Navigation;
using VKSaver.Common;
using VKSaver.Core.ViewModels;

namespace VKSaver.Controls
{
    public sealed partial class SimpleShell : UserControl
    {
        public SimpleShell()
        {
            this.InitializeComponent();

            WindowThemeHelper.HideTitleBar();
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            var connectedAnimationService = ConnectedAnimationService.GetForCurrentView();
            connectedAnimationService.DefaultDuration = TimeSpan.FromSeconds(0.4);
            connectedAnimationService.DefaultEasingFunction = _compositor.CreateCubicBezierEasingFunction(
                new Vector2(0.41f, 0.52f),
                new Vector2(0.00f, 0.94f)
                );

            this.SizeChanged += SimpleShell_SizeChanged;
        }

        private void SimpleShell_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            WindowSizeText.Text = $"{e.NewSize.Width}x{e.NewSize.Height}";
        }

        [InjectionConstructor]
        public SimpleShell(
            INavigationService navigationService, 
            PlayerViewModel playerViewModel)
            : this()
        {
            _navigationService = navigationService;

            playerViewModel.OnNavigatedTo(null, null);
            PlayerBlock.DataContext = playerViewModel;
        }

        public Frame CurrentFrame
        {
            get { return ContentBorder.Child as Frame; }
            set
            {
                var oldFrame = ContentBorder.Child as Frame;
                if (oldFrame != null)
                {
                    oldFrame.Navigated -= CurentFrame_Navigated;
                }

                value.Navigated += CurentFrame_Navigated;
                ContentBorder.Child = value;
            }
        }

        public static readonly DependencyProperty IsPlayerBlockVisibleProperty = DependencyProperty.RegisterAttached(
            "IsPlayerBlockVisible", typeof(bool), typeof(SimpleShell), new PropertyMetadata(true, 
                (s, e) => GetCurrentShell()?.UpdatePlayerBlock()));

        public static void SetIsPlayerBlockVisible(DependencyObject element, bool value)
        {
            element.SetValue(IsPlayerBlockVisibleProperty, value);
        }

        public static bool GetIsPlayerBlockVisible(DependencyObject element)
        {
            return (bool)element.GetValue(IsPlayerBlockVisibleProperty);
        }

        private void CurentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            UpdatePlayerBlock();
        }

        private void TrackBlock_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("TrackBlock", TrackBlock);

            _navigationService.Navigate("PlayerView", null);
        }

        private void UpdatePlayerBlock()
        {
            if (CurrentFrame.Content == null)
                return;

            if (GetIsPlayerBlockVisible(CurrentFrame.Content as DependencyObject))
            {
                ShowPlayerBlockStoryboard.Begin();
            }
            else
            {
                HidePlayerBlockStoryboard.Begin();
            }
        }

        private static SimpleShell GetCurrentShell()
        {
            return Window.Current.Content as SimpleShell;
        }
        
        private readonly Compositor _compositor;
        private readonly INavigationService _navigationService;
    }
}
