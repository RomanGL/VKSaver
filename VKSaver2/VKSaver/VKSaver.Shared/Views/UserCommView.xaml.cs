using Microsoft.Practices.Prism.StoreApps;

namespace VKSaver.Views
{
    /// <summary>
    /// Представление информации о треке.
    /// </summary>
    public sealed partial class UserCommView : VisualStateAwarePage
    {
#if WINDOWS_APP
        /// <summary>
        /// Помощник навигации.
        /// </summary>
        public WindowsNavigationHelper VKSaver.Helpers.WindowsNavigationHelper { get; private set; }
#endif

        public UserCommView()
        {
            this.InitializeComponent();
#if WINDOWS_APP
            WindowsNavigationHelper = new VKSaver.Helpers.WindowsNavigationHelper(this);
#endif
        }
    }
}
