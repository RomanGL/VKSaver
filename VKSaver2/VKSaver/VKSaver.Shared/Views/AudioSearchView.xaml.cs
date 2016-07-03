using Microsoft.Practices.Prism.StoreApps;
using VKSaver.Core.ViewModels;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Views
{
    public sealed partial class AudioSearchView : VisualStateAwarePage
    {
        public AudioSearchView()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.Loaded += AudioSearchView_Loaded;
        }

        private void AudioSearchView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= AudioSearchView_Loaded;
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
