using Microsoft.Practices.Prism.StoreApps;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Views
{
    public partial class AdInfoView : VisualStateAwarePage
    {
        public AdInfoView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            background.Start();
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            background.Stop();
            base.OnNavigatedFrom(e);
        }
    }
}
