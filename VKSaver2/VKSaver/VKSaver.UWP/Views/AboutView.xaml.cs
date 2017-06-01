using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using Prism.Windows.Mvvm;

namespace VKSaver.Views
{
    public sealed partial class AboutView : SessionStateAwarePage
    {
        public AboutView()
        {
            this.InitializeComponent();
            this.Loaded += AboutView_Loaded;
        }

        private void AboutView_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Loaded -= AboutView_Loaded;
            FadeInAnimation.Begin();
        }
    }
}
