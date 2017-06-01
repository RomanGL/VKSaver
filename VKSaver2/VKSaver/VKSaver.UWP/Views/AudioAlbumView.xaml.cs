using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Prism.Windows.Mvvm;
using VKSaver.Common;
using VKSaver.Core.ViewModels;

namespace VKSaver.Views
{
    public sealed partial class AudioAlbumView : SessionStateAwarePage
    {
        public AudioAlbumView()
        {
            this.InitializeComponent();
            this.Loaded += AudioAlbumView_Loaded;
            AudiosListView.Loaded += AudiosListView_Loaded;
        }

        private AudioAlbumViewModel VM => this.DataContext as AudioAlbumViewModel;

        private void AudioAlbumView_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Loaded -= AudioAlbumView_Loaded;
            FadeInAnimation.Begin();
        }

        private void AudiosListView_Loaded(object sender, RoutedEventArgs e)
        {
            AudiosListView.Loaded -= AudiosListView_Loaded;

            var audiosScroll = AudiosListView.GetScrollViewer();
            audiosScroll.ChangeView(null, VM?.AudiosScrollPosition, null, true);
            audiosScroll.ViewChanged += (s, args) =>
            {
                if (VM != null)
                    VM.AudiosScrollPosition = audiosScroll.VerticalOffset;
            };
        }
    }
}
