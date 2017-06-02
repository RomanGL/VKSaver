using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using Prism.Windows.Mvvm;

namespace VKSaver.Views
{
    public sealed partial class ErrorView : SessionStateAwarePage
    {
        public ErrorView()
        {
            this.InitializeComponent();
            this.Loaded += ErrorView_Loaded;
        }

        private void ErrorView_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Loaded -= ErrorView_Loaded;
            FadeInAnimation.Begin();
        }
    }
}
