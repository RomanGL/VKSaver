using Microsoft.Practices.Prism.StoreApps;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Views
{
    /// <summary>
    /// Предеставления списка топовых треков Last.fm.
    /// </summary>
    public sealed partial class TopTracksView : VisualStateAwarePage
    {
#if WINDOWS_APP
        /// <summary>
        /// Помощник навигации.
        /// </summary>
        public WindowsNavigationHelper VKSaver.Helpers.WindowsNavigationHelper { get; private set; }
#endif

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public TopTracksView()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
#if WINDOWS_APP
            WindowsNavigationHelper = new VKSaver.Helpers.WindowsNavigationHelper(this);
#endif
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
        }
    }
}
