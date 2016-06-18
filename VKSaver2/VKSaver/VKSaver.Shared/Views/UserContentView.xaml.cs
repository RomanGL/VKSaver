using Microsoft.Practices.Prism.StoreApps;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента пустой страницы см. по адресу http://go.microsoft.com/fwlink/?LinkID=390556

namespace VKSaver.Views
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class UserContentView : VisualStateAwarePage
    {
#if WINDOWS_APP
        /// <summary>
        /// Помощник навигации.
        /// </summary>
        public WindowsNavigationHelper VKSaver.Helpers.WindowsNavigationHelper { get; private set; }
#endif

        public UserContentView()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
#if WINDOWS_APP
            WindowsNavigationHelper = new VKSaver.Helpers.WindowsNavigationHelper(this);
#endif
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
                NavigationCacheMode = NavigationCacheMode.Disabled;
            base.OnNavigatedFrom(e);
        }
    }
}
