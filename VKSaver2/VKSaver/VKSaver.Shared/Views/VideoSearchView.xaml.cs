using Microsoft.Practices.Prism.StoreApps;
using VKSaver.Core.ViewModels;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Views
{
    public sealed partial class VideoSearchView : VisualStateAwarePage
    {
        public VideoSearchView()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.Loaded += VideoSearchView_Loaded;
        }

        private void VideoSearchView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= VideoSearchView_Loaded;
            SearchBox.Focus(FocusState.Programmatic);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        private void FilterFlyoutOpenning(object sender, object e)
        {
            this.BottomAppBar.Visibility = Visibility.Collapsed;
        }

        private void FilterFlyoutClosed(object sender, object e)
        {
            this.BottomAppBar.Visibility = Visibility.Visible;
            ((VideoSearchViewModel)DataContext).FilterFlyoutClosedCommand.Execute();
        }
    }
}
