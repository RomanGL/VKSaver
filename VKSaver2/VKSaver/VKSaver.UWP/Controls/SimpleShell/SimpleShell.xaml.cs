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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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

            NavigationItems = GetNavigationCollection();
            HeadersList.ItemsSource = NavigationItems;
            HeadersList.SelectionChanged += HeadersList_SelectionChanged;
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

        public ObservableCollection<ShellNavigationItem> NavigationItems { get; set; }

        public static void SetIsPlayerBlockVisible(DependencyObject element, bool value)
        {
            element.SetValue(IsPlayerBlockVisibleProperty, value);
        }

        public static bool GetIsPlayerBlockVisible(DependencyObject element)
        {
            return (bool)element.GetValue(IsPlayerBlockVisibleProperty);
        }

        public static void SetIsMenuVisible(DependencyObject element, bool value)
        {
            element.SetValue(IsMenuVisibleProperty, value);
        }

        public static bool GetIsMenuVisible(DependencyObject element)
        {
            return (bool)element.GetValue(IsMenuVisibleProperty);
        }

        public static readonly DependencyProperty IsPlayerBlockVisibleProperty = DependencyProperty.RegisterAttached(
            "IsPlayerBlockVisible", typeof(bool), typeof(SimpleShell), new PropertyMetadata(true, 
                (s, e) => GetCurrentShell()?.UpdatePlayerBlock()));

        public static readonly DependencyProperty IsMenuVisibleProperty = DependencyProperty.RegisterAttached(
            "IsMenuVisible", typeof(bool), typeof(SimpleShell), new PropertyMetadata(true,
                (s, e) => GetCurrentShell()?.UpdateMenuBlock()));

        private void CurentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            UpdatePlayerBlock();
            UpdateMenuBlock();

            HeadersList.SelectionChanged -= HeadersList_SelectionChanged;

            string destinationView = e.SourcePageType.Name;
            var item = NavigationItems.FirstOrDefault(n => n.DestinationView == destinationView);
            HeadersList.SelectedIndex = item == null ? -1 : NavigationItems.IndexOf(item);

            HeadersList.SelectionChanged += HeadersList_SelectionChanged;
        }

        private void HeadersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = NavigationItems[HeadersList.SelectedIndex];
            _navigationService.Navigate(item.DestinationView, item.NavigationParameter);
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

        private void UpdateMenuBlock()
        {
            if (CurrentFrame.Content == null)
                return;

            if (GetIsPlayerBlockVisible(CurrentFrame.Content as DependencyObject))
                HeaderBlock.Visibility = Visibility.Visible;
            else
                HeaderBlock.Visibility = Visibility.Collapsed;
        }

        private static ObservableCollection<ShellNavigationItem> GetNavigationCollection()
        {
            return new ObservableCollection<ShellNavigationItem>
            {
                new ShellNavigationItem { Name = "Главная", DestinationView = "MainView" },
                new ShellNavigationItem { Name = "Коллекция", DestinationView = "LibraryView" }
            };
        }

        private static SimpleShell GetCurrentShell()
        {
            return Window.Current.Content as SimpleShell;
        }
        
        private readonly Compositor _compositor;
        private readonly INavigationService _navigationService;
    }
}
