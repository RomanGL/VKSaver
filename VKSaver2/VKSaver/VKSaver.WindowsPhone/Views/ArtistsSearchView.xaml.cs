using Microsoft.Practices.Prism.StoreApps;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Views
{
    public sealed partial class ArtistsSearchView : VisualStateAwarePage
    {
        public ArtistsSearchView()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.Loaded += ArtistsSearchView_Loaded;
        }

        private void ArtistsSearchView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= ArtistsSearchView_Loaded;
            SearchBox.Focus(FocusState.Programmatic);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
        }
    }
}
