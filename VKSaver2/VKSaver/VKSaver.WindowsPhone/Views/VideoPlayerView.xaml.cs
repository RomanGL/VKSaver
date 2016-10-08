using Microsoft.Practices.ServiceLocation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.StoreApps;

namespace VKSaver.Views
{
    /// <summary>
    /// Представление информации о треке.
    /// </summary>
    public sealed partial class VideoPlayerView : VisualStateAwarePage
    {
#if WINDOWS_APP
        /// <summary>
        /// Помощник навигации.
        /// </summary>
        public WindowsNavigationHelper VKSaver.Helpers.WindowsNavigationHelper { get; private set; }
#endif

        public VideoPlayerView()
        {
            this.InitializeComponent();
#if WINDOWS_APP
            WindowsNavigationHelper = new VKSaver.Helpers.WindowsNavigationHelper(this);
#endif
        }
    }
}
