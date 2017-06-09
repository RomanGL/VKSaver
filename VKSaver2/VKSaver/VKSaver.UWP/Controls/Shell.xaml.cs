using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Windows.ApplicationModel.Core;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Unity;
using Prism.Windows.Navigation;
using VKSaver.Common;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels;
using VKSaver.Core.Models;

namespace VKSaver.Controls
{
    public sealed partial class Shell : UserControl, IAppNotificationsPresenter
    {
        [InjectionConstructor]
        public Shell(INavigationService navigationService)
            : this()
        {
            _navigationService = navigationService;
        }

        public Shell()
        {
            this.InitializeComponent();

            WindowThemeHelper.HideTitleBar();
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            ShellSplitView.PaneClosed += ShellSplitView_PaneClosed;
            this.SizeChanged += Shell_SizeChanged;
            MenuButton.Click += MenuButton_Click;

            Window.Current.CoreWindow.Activated += (sender, args) =>
            {
                if (args.WindowActivationState == CoreWindowActivationState.Deactivated)
                {
                    PaneBlur.Value = 0;
                    WindowBlur.Value = 0;
                }
                else
                {
                    PaneBlur.Value = 1;
                    WindowBlur.Value = 1;
                }
            };
            
            this.Loaded += Shell_Loaded;
            MenuListView.ItemsSource = _navigationItems;
            MenuListView.ItemClick += NavigationListView_ItemClick;

            BottomMenuListView.ItemsSource = _bottmNavigationItems;
            BottomMenuListView.ItemClick += NavigationListView_ItemClick;

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

        public PlayerViewModel PlayerVM
        {
            get { return _playerVM; }
            set
            {
                _playerVM = value;
                PlayerBlock.DataContext = _playerVM;
                _playerVM.OnNavigatedTo(null, null);
            }
        }

        #region DependencyProperties

        public static void SetChromeStyle(DependencyObject element, ChromeStyle value)
        {
            element.SetValue(ChromeStyleProperty, value);
        }

        public static ChromeStyle GetChromeStyle(DependencyObject element)
        {
            return (ChromeStyle)element.GetValue(ChromeStyleProperty);
        }

        public static void SetIsPlayerBlockVisible(DependencyObject element, bool value)
        {
            element.SetValue(IsPlayerBlockVisibleProperty, value);
        }

        public static bool GetIsPlayerBlockVisible(DependencyObject element)
        {
            return (bool)element.GetValue(IsPlayerBlockVisibleProperty);
        }

        public static readonly DependencyProperty IsPlayerBlockVisibleProperty = 
            DependencyProperty.RegisterAttached("IsPlayerBlockVisible", typeof(bool), 
                typeof(Shell), new PropertyMetadata(true,
                    (s, e) => GetCurrentShell()?.UpdatePlayerBlock()));

        public static readonly DependencyProperty ChromeStyleProperty = 
            DependencyProperty.RegisterAttached("ChromeStyle", typeof(ChromeStyle), 
                typeof(Shell), new PropertyMetadata(default(ChromeStyle),
                    (s, e) => GetCurrentShell()?.UpdateSplitViewState()));

        #endregion

        public async void ShowNotification(AppNotification notification)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (notification.IsHided)
                    return;

                if (NotificationsPanel == null)
                {
                    lock (_lockObject)
                    {
                        _waitingNotifications.Add(notification);
                        if (_waitingNotifications.Count >= 5)
                            _waitingNotifications.RemoveAt(0);

                        return;
                    }
                }

                var anc = new AppNotificationControl { Message = notification };
                anc.Loaded += (s, e) => anc.Show();
                anc.Tapped += (s, e) =>
                {
                    lock (_lockObject)
                        _notificationsCount--;

                    anc.Hide();
                    if (!String.IsNullOrWhiteSpace(anc.Message.DestinationView))
                        _navigationService.Navigate(anc.Message.DestinationView, anc.Message.NavigationParameter);

                    anc.Message.ActionToDo?.Invoke();
                };
                notification.HideRequested += (s, e) =>
                {
                    lock (_lockObject)
                        _notificationsCount--;
                    anc.Hide();
                };

                lock (_lockObject)
                {
                    NotificationsPanel.Children.Insert(0, anc);
                    _notificationsCount++;

                    if (_notificationsCount >= 5)
                    {
                        //var notificationControl = NotificationsPanel.Children.LastOrDefault() as AppNotificationControl;
                        NotificationsPanel.Children.RemoveAt(NotificationsPanel.Children.Count - 1);
                        _notificationsCount--;
                    }
                }

                Debug.WriteLine($"Count: {_notificationsCount}");
            });
        }

        private void Shell_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= Shell_Loaded;
            if (_waitingNotifications.Count > 0)
            {
                foreach (var notification in _waitingNotifications)
                    ShowNotification(notification);
                _waitingNotifications.Clear();
            }
        }

        private void CurentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            ShellSplitView.IsPaneOpen = false;

            if (_currentNavigationItem != null)
                _currentNavigationItem.IsCurrent = false;

            string viewName = e.SourcePageType.Name;

            _currentNavigationItem = _navigationItems.FirstOrDefault(item => item.DestinationView == viewName);
            if (_currentNavigationItem == null)
                _currentNavigationItem = _bottmNavigationItems.FirstOrDefault(item => item.DestinationView == viewName);

            if (_currentNavigationItem != null)
                _currentNavigationItem.IsCurrent = true;

            UpdatePlayerBlock();
            UpdateSplitViewState();
            UpdateSplitViewPaneBackground();
        }

        private void TrackBlock_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            //ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("TrackBlock", TrackBlock);
            _navigationService.Navigate("PlayerView", null);
        }

        private void NavigationListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (ShellNavigationItem)e.ClickedItem;
            _navigationService.Navigate(item.DestinationView, item.NavigationParameter);
        }

        private void Shell_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //UpdateSplitViewState();
        }

        private void ShellSplitView_PaneClosed(SplitView sender, object args)
        {
            UpdateSplitViewPaneBackground();
        }

        private void UpdateSplitViewPaneBackground()
        {
            //if (ShellSplitView.IsPaneOpen)
            //{
            //    ShellSplitView.PaneBackground = 
            //        App.Current.Resources["SystemControlBackgroundChromeMediumBrush"] as Brush;
            //}
            //else
            //{
            //    ShellSplitView.PaneBackground =
            //        App.Current.Resources["TextControlPlaceholderForegroundFocused"] as Brush;
            //}
        }

        private void UpdatePlayerBlock()
        {
            if (CurrentFrame.Content == null)
                return;

            if (GetIsPlayerBlockVisible(CurrentFrame.Content as DependencyObject))
            {
                ShowPlayerBlockStoryboard.Begin();
                BottomMenuListView.Margin = new Thickness(0, 0, 0, 60);
            }
            else
            {
                HidePlayerBlockStoryboard.Begin();
                BottomMenuListView.Margin = new Thickness(0);
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            ShellSplitView.IsPaneOpen = !ShellSplitView.IsPaneOpen;
            UpdateSplitViewPaneBackground();
        }

        private void UpdateSplitViewState()
        {
            if (CurrentFrame.Content == null)
                return;

            var state = GetChromeStyle((DependencyObject)CurrentFrame.Content);
            switch (state)
            {
                case ChromeStyle.Compact:
                    MenuButton.Visibility = Visibility.Visible;
                    ShellSplitView.DisplayMode = SplitViewDisplayMode.CompactOverlay;
                    ShellSplitView.IsPaneOpen = false;
                    PaneBlur.Value = 1;
                    break;
                case ChromeStyle.OnlyButton:
                    MenuButton.Visibility = Visibility.Visible;
                    ShellSplitView.DisplayMode = SplitViewDisplayMode.Overlay;
                    ShellSplitView.IsPaneOpen = false;
                    PaneBlur.Value = 0;
                    break;
                case ChromeStyle.Hided:
                    MenuButton.Visibility = Visibility.Collapsed;
                    ShellSplitView.DisplayMode = SplitViewDisplayMode.Overlay;
                    ShellSplitView.IsPaneOpen = false;
                    PaneBlur.Value = 1;
                    break;
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
                new ShellNavigationItem { Name = "Домашняя страница", DestinationView = "MainView", Icon = "\uE10F"},
                new ShellNavigationItem { Name = "Локальная библиотека", DestinationView = "LibraryView", Icon = "\uE838"},
                new ShellNavigationItem { Name = "Музыка", DestinationView = "UserContentView", NavigationParameter = "{\"Key\":\"audios\",\"Value\":\"0\"}", Icon = "\uE189"}
            };
        }

        private static ObservableCollection<ShellNavigationItem> GetBottomNavigationCollection()
        {
            return new ObservableCollection<ShellNavigationItem>
            {
                new ShellNavigationItem {Name = "Менеджер загрузок", DestinationView = "TransferView", Icon = "\uE118"},
                new ShellNavigationItem { Name = "Настройки", DestinationView = "SettingsView", Icon = "\uE115"},
                new ShellNavigationItem { Name = "О программе", DestinationView = "AboutView", Icon = "\uE783"}
            };
        }

        private ShellNavigationItem _currentNavigationItem;

        private readonly List<AppNotification> _waitingNotifications = new List<AppNotification>();
        private readonly ObservableCollection<ShellNavigationItem> _navigationItems = GetNavigationCollection();
        private readonly ObservableCollection<ShellNavigationItem> _bottmNavigationItems = GetBottomNavigationCollection();
        private readonly Compositor _compositor;
        private readonly INavigationService _navigationService;
        private readonly object _lockObject = new object();

        private PlayerViewModel _playerVM;
        private int _notificationsCount = 0;

        public enum ChromeStyle
        {
            Compact,
            OnlyButton,
            Hided
        }
    }
}
