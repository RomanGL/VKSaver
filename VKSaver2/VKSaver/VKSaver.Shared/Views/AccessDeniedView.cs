using Microsoft.Practices.Prism.StoreApps;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Views
{
    public partial class NoTracksView : VisualStateAwarePage
    {
        public NoTracksView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
#if WINDOWS_PHONE_APP
            background.Start();
#endif
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
#if WINDOWS_PHONE_APP
            background.Stop();
#endif
            base.OnNavigatedFrom(e);
        }
    }
}
