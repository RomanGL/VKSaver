using Microsoft.Practices.Prism.StoreApps;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Views
{
    public sealed partial class NewsMediaView : VisualStateAwarePage
    {
        public NewsMediaView()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
                NavigationCacheMode = NavigationCacheMode.Disabled;

            base.OnNavigatedFrom(e);
        }
    }
}
