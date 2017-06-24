using Prism.Windows.Mvvm;

namespace VKSaver.Views
{
    public sealed partial class VideoInfoView : SessionStateAwarePage
    {
        public VideoInfoView()
        {
            this.InitializeComponent();
            this.Loaded += VideoInfoView_Loaded;
        }

        private void VideoInfoView_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Loaded -= VideoInfoView_Loaded;
            FadeInAnimation.Begin();
        }
    }
}
