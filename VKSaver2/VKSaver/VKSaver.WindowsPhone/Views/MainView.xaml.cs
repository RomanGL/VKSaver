using Microsoft.Practices.Prism.StoreApps;
using VKSaver.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Views
{
    /// <summary>
    /// Представление главной страницы.
    /// </summary>
    public sealed partial class MainView : VisualStateAwarePage
    {
        public MainView()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {                
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
                this.NavigationCacheMode = NavigationCacheMode.Required;
                RootHub.ScrollToSection(RootHub.Sections[0]);
            }

            base.OnNavigatedTo(e);
        }
    }
}
