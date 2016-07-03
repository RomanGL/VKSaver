using Microsoft.Practices.Prism.StoreApps;
using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Views
{
    public partial class BetaBlockerView : VisualStateAwarePage
    {
        public BetaBlockerView()
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

        private async void OpenVKSaverStorePage_Click(object sender, RoutedEventArgs e)
        {
            //await Launcher.LaunchUriAsync(new Uri("ms-windows-store://pdp/?ProductId=9wzdncrdr1p3"));
            await Launcher.LaunchUriAsync(new Uri("http://windowsphone.com/s?appid=***REMOVED***"));
        }
    }
}
