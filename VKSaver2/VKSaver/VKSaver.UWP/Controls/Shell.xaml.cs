using System;
using System.Collections.ObjectModel;
using System.Numerics;
using Windows.ApplicationModel.Core;
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
    public sealed partial class Shell : UserControl
    {
        [InjectionConstructor]
        public Shell(INavigationService navigationService, PlayerViewModel playerViewModel)
            : this()
        {
            _navigationService = navigationService;

            PlayerBlock.DataContext = playerViewModel;
            playerViewModel.OnNavigatedTo(null, null);
        }

        public Shell()
        {
            this.InitializeComponent();

            MenuListBox.ItemsSource = _navigationItems;
            MenuListBox.SelectionChanged += MenuListBox_SelectionChanged;
            MenuButton.Click += (s, e) => ShellSplitView.IsPaneOpen = !ShellSplitView.IsPaneOpen;
            this.SizeChanged += Shell_SizeChanged;
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = false;

            WindowThemeHelper.HideTitleBar();
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            var connectedAnimationService = ConnectedAnimationService.GetForCurrentView();
            connectedAnimationService.DefaultDuration = TimeSpan.FromSeconds(0.4);
            connectedAnimationService.DefaultEasingFunction = _compositor.CreateCubicBezierEasingFunction(
                new Vector2(0.41f, 0.52f),
                new Vector2(0.00f, 0.94f));
        }

        public Frame CurrentFrame
        {
            get { return ContentBorder.Child as Frame; }
            set
            {
                var oldFrame = ContentBorder.Child as Frame;
                if (oldFrame != null)
                    oldFrame.Navigated -= CurentFrame_Navigated;

                value.Navigated += CurentFrame_Navigated;
                ContentBorder.Child = value;
            }
        }
        
        #region DependencyProperties
        
        public static string GetTitle(DependencyObject obj)
        {
            return (string)obj.GetValue(TitleProperty);
        }

        public static void SetTitle(DependencyObject obj, string value)
        {
            obj.SetValue(TitleProperty, value);
        }

        public static bool GetIsMenuButtonVisible(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMenuButtonVisibleProperty);
        }

        public static void SetIsMenuButtonVisible(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMenuButtonVisibleProperty, value);
        }

        public static bool GetIsTitleBarVisible(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsTitleBarVisibleProperty);
        }

        public static void SetIsTitleBarVisible(DependencyObject obj, bool value)
        {
            obj.SetValue(IsTitleBarVisibleProperty, value);
        }

        public static void SetIsPlayerBlockVisible(DependencyObject element, bool value)
        {
            element.SetValue(IsPlayerBlockVisibleProperty, value);
        }

        public static bool GetIsPlayerBlockVisible(DependencyObject element)
        {
            return (bool)element.GetValue(IsPlayerBlockVisibleProperty);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.RegisterAttached("Title", typeof(string),
                typeof(Shell), new PropertyMetadata(default(string), (x, e) => GetCurrentShell()?.UpdatePageTitle()));

        public static readonly DependencyProperty IsMenuButtonVisibleProperty =
            DependencyProperty.RegisterAttached("IsMenuButtonVisible", typeof(bool),
                typeof(Shell), new PropertyMetadata(true, (s, e) => GetCurrentShell()?.UpdateMenuButton()));

        public static readonly DependencyProperty IsTitleBarVisibleProperty =
            DependencyProperty.RegisterAttached("IsTitleBarVisible", typeof(bool),
                typeof(Shell), new PropertyMetadata(true, (s, e) => GetCurrentShell()?.UpdateTitleBar()));

        public static readonly DependencyProperty IsPlayerBlockVisibleProperty = 
            DependencyProperty.RegisterAttached("IsPlayerBlockVisible", typeof(bool), 
                typeof(Shell), new PropertyMetadata(true,
                (s, e) => GetCurrentShell()?.UpdatePlayerBlock()));

        #endregion

        private void CurentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            UpdatePlayerBlock();
            UpdatePageTitle();
            UpdateMenuButton();
            UpdateTitleBar();
            UpdateSplitViewState();
        }

        private void TrackBlock_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("TrackBlock", TrackBlock);
            _navigationService.Navigate("PlayerView", null);
        }

        private void MenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MenuListBox.SelectedItem == null)
                return;

            var item = (ShellNavigationItem)MenuListBox.SelectedItem;
            if (item.DestinationView == "Hamburger")
            {
                ShellSplitView.IsPaneOpen = !ShellSplitView.IsPaneOpen;
                MenuListBox.SelectedIndex = -1;
            }
            else
                _navigationService.Navigate(item.DestinationView, item.NavigationParameter);
        }

        private void Shell_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSplitViewState();
        }

        private void UpdatePageTitle()
        {
            if (CurrentFrame.Content == null)
                return;
            
            string title = GetTitle(CurrentFrame.Content as DependencyObject) ?? String.Empty;
            PageTitle.Text = title;
            _navigationItems[0].Name = title;
        }

        private void UpdateMenuButton()
        {
            if (CurrentFrame.Content == null)
                return;

            if (GetIsMenuButtonVisible(CurrentFrame.Content as DependencyObject))
            {
                ShowMenuButtonStoryboard.Begin();
            }
            else
            {
                HideMenuButtonStoryboard.Begin();
            }
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

        private void UpdateTitleBar()
        {
            if (CurrentFrame.Content == null)
                return;

            if (GetIsTitleBarVisible(CurrentFrame.Content as DependencyObject))
            {
                ShowTitleBarStoryboard.Begin();
            }
            else
            {
                HideTitleBarStoryboard.Begin();
            }
        }

        private void UpdateSplitViewState()
        {
            if (CurrentFrame.Content == null)
                return;

            bool flag = GetIsMenuButtonVisible(CurrentFrame.Content as DependencyObject);

            if (flag && this.ActualWidth >= 600)
            {
                ShellSplitView.DisplayMode = SplitViewDisplayMode.CompactOverlay;
                MenuButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShellSplitView.DisplayMode = SplitViewDisplayMode.Overlay;
                MenuButton.Visibility = Visibility.Visible;
            }
        }

        private static Shell GetCurrentShell()
        {
            return Window.Current.Content as Shell;
        }

        private static ObservableCollection<ShellNavigationItem> GetNavigationCollection()
        {
            return new ObservableCollection<ShellNavigationItem>
            {
                new ShellNavigationItem { DestinationView = "Hamburger", Icon = "\uE700"},
                new ShellNavigationItem { Name = "Главная", DestinationView = "MainView", Icon = "\uE10F"},
                new ShellNavigationItem { Name = "Коллекция", DestinationView = "LibraryView", Icon = "\uE838"}
            };
        }

        private readonly ObservableCollection<ShellNavigationItem> _navigationItems = GetNavigationCollection();
        private readonly Compositor _compositor;
        private readonly INavigationService _navigationService;
    }
}
