using System;
using System.Linq;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using ModernDev.InTouch;
using Prism.Windows.Mvvm;
using VKSaver.Common;
using VKSaver.Core.ViewModels;

namespace VKSaver.Views
{
    public sealed partial class MainView : SessionStateAwarePage
    {
        public MainView()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void BackButton_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!Frame.CanGoBack)
                ((AppBarButton)sender).Visibility = Visibility.Collapsed;
        }
    }
}
