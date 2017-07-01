using Microsoft.Practices.Prism.StoreApps;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Views
{
    /// <summary>
    /// Специальное представление главной страницы.
    /// </summary>
    public sealed partial class SpecialMainView : VisualStateAwarePage
    {
        public SpecialMainView()
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
