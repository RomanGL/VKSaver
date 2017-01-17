using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.StoreApps;
using VKSaver.Core.ViewModels;

namespace VKSaver.Views
{
    public sealed partial class PopularVKAudioView : VisualStateAwarePage
    {
        public PopularVKAudioView()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
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
            BottomAppBar.Visibility = Visibility.Visible;
            ((PopularVKAudioViewModel)DataContext).FilterFlyoutClosedCommand.Execute();
        }

        private void ComboBox_OnDropDownClosed(object sender, object e)
        {
            BottomAppBar.Visibility = Visibility.Visible;
            ((PopularVKAudioViewModel)DataContext).FilterFlyoutClosedCommand.Execute();
        }
    }
}
