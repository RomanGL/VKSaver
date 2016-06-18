using Microsoft.Practices.Prism.StoreApps;
using VKSaver.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Views
{
    /// <summary>
    /// Представляет страницу со списком топовых исполнителей.
    /// </summary>
    public sealed partial class TopArtistsView : VisualStateAwarePage
    {
#if WINDOWS_APP
        /// <summary>
        /// Помощник навигации.
        /// </summary>
        public WindowsNavigationHelper VKSaver.Helpers.WindowsNavigationHelper { get; private set; }
#endif
        /// <summary>
        /// Инциализирует новый экземпляр страницы со списком топовых исполнителей.
        /// </summary>
        public TopArtistsView()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
#if WINDOWS_APP
            WindowsNavigationHelper = new VKSaver.Helpers.WindowsNavigationHelper(this);
#endif
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            base.OnNavigatingFrom(e);
        }
    }
}
