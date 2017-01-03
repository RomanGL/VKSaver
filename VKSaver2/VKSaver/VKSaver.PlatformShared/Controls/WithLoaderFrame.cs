using Microsoft.Practices.Prism.StoreApps.Interfaces;
using System;
using System.Collections.Generic;
using VKSaver.Core.Models;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Interfaces;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace VKSaver.Controls
{
    public sealed class WithLoaderFrame : Frame, ILoader, IAppNotificationsPresenter
    {
        public WithLoaderFrame()
        {
            this.DefaultStyleKey = typeof(WithLoaderFrame);
        }

        public WithLoaderFrame(
            IAppLoaderService appLoaderService, 
            IServiceResolver serviceResolver)
        {
            this.DefaultStyleKey = typeof(WithLoaderFrame);
            AppLoaderService = appLoaderService;

            _serviceResolver = serviceResolver;
            _navigationService = new Lazy<INavigationService>(() => _serviceResolver.Resolve<INavigationService>());
        }

        public IAppLoaderService AppLoaderService { get; private set; }

        public string LoaderText
        {
            get { return (string)GetValue(LoaderTextProperty); }
            set { SetValue(LoaderTextProperty, value); }
        }
        
        public static readonly DependencyProperty LoaderTextProperty =
            DependencyProperty.Register("LoaderText", typeof(string), typeof(WithLoaderFrame), new PropertyMetadata(null));

        public async void HideLoader()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                VisualStateManager.GoToState(this, "Normal", true);

                var page = Content as Page;
                if (page != null && page.BottomAppBar != null && _appBarHided)
                {
                    _appBarHided = false;
                    page.BottomAppBar.Visibility = Visibility.Visible;
                    page.BottomAppBar.Tag = null;
                }
            });
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            var page = Content as Page;
            if (page != null && page.BottomAppBar != null && _appBarHided)
            {
                _appBarHided = false;
                page.BottomAppBar.Visibility = Visibility.Visible;
                page.BottomAppBar.Tag = null;
            }

            base.OnContentChanged(oldContent, newContent);
        }

        public async void ShowLoader(string text)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                LoaderText = text;
                VisualStateManager.GoToState(this, "Showed", true);

                var page = Content as Page;
                if (page != null && page.BottomAppBar != null && page.BottomAppBar.Visibility == Visibility.Visible)
                {
                    page.BottomAppBar.Visibility = Visibility.Collapsed;
                    page.BottomAppBar.Tag = true;
                    _appBarHided = true;
                }
            });
        }

        public async void ShowNotification(AppNotification notification)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (notification.IsHided)
                    return;

                if (_notificationsPanel == null)
                {
                    _waitingNotifications.Add(notification);
                    return;
                }

                var anc = new AppNotificationControl() { Message = notification };
                anc.Loaded += (s, e) => anc.Show();
                anc.Tapped += (s, e) =>
                {
                    anc.Hide();
                    if (!String.IsNullOrWhiteSpace(anc.Message.DestinationView))
                        _navigationService.Value.Navigate(anc.Message.DestinationView, anc.Message.NavigationParameter);

                    anc.Message.ActionToDo?.Invoke();
                };
                notification.HideRequested += (s, e) => anc.Hide();
                _notificationsPanel.Children.Insert(0, anc);
            });
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (ContentTransitions == null)
            {
                ContentTransitions = new TransitionCollection
                {
                    new NavigationThemeTransition()
                };
            }

            _notificationsPanel = GetTemplateChild("NotificationsPanel") as Panel;

            this.Loaded += WithLoaderFrame_Loaded;
            this.Unloaded += WithLoaderFrame_Unloaded;

            VisualStateManager.GoToState(this, "Normal", true);

            if (_waitingNotifications.Count > 0)
            {
                foreach (var notification in _waitingNotifications)
                    ShowNotification(notification);
                _waitingNotifications.Clear();
            }
        }

        private void WithLoaderFrame_Loaded(object sender, RoutedEventArgs e)
        {
            AppLoaderService?.AddLoader(this);
        }

        private void WithLoaderFrame_Unloaded(object sender, RoutedEventArgs e)
        {
            AppLoaderService?.RemoveLoader(this);
        }

        private bool _appBarHided;
        private Panel _notificationsPanel;
        private readonly List<AppNotification> _waitingNotifications = new List<AppNotification>();
        private readonly Lazy<INavigationService> _navigationService;
        private readonly IServiceResolver _serviceResolver;
    }
}
