using Windows.UI.Xaml;
using Prism.Windows.Mvvm;
using VKSaver.Common;
using VKSaver.Core.ViewModels;

namespace VKSaver.Views
{
    public sealed partial class UserContentView : SessionStateAwarePage
    {
        public UserContentView()
        {
            this.InitializeComponent();
            
            AudiosListView.Loaded += AudiosListView_Loaded;
            VideosGridView.Loaded += VideosGridView_Loaded;
            DocsListView.Loaded += DocsListView_Loaded;
        }

        private UserContentViewModel VM => this.DataContext as UserContentViewModel;
        
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

        private void VideosGridView_Loaded(object sender, RoutedEventArgs e)
        {
            VideosGridView.Loaded -= VideosGridView_Loaded;

            var videosScroll = VideosGridView.GetScrollViewer();
            videosScroll.ChangeView(null, VM?.VideosScrollPosition, null, true);
            videosScroll.ViewChanged += (s, args) =>
            {
                if (VM != null)
                    VM.VideosScrollPosition = videosScroll.VerticalOffset;
            };
        }

        private void DocsListView_Loaded(object sender, RoutedEventArgs e)
        {
            DocsListView.Loaded -= DocsListView_Loaded;

            var docsScroll = DocsListView.GetScrollViewer();
            docsScroll.ChangeView(null, VM?.DocsScrollPosition, null, true);
            docsScroll.ViewChanged += (s, args) =>
            {
                if (VM != null)
                    VM.DocsScrollPosition = docsScroll.VerticalOffset;
            };
        }
    }
}
