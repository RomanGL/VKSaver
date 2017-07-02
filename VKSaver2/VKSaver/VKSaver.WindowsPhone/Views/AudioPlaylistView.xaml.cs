using Microsoft.Practices.Prism.StoreApps;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Views
{
    public sealed partial class AudioPlaylistView : VisualStateAwarePage
    {
        public AudioPlaylistView()
        {
            InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
        }
    }
}
