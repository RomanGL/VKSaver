using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using Prism.Windows.Mvvm;

namespace VKSaver.Views
{
    public sealed partial class VKAudioInfoView : SessionStateAwarePage
    {
        public VKAudioInfoView()
        {
            this.InitializeComponent();
            this.Loaded += VKAudioInfoView_Loaded;
        }

        private void VKAudioInfoView_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Loaded -= VKAudioInfoView_Loaded;
            FadeInAnimation.Begin();
        }
    }
}
