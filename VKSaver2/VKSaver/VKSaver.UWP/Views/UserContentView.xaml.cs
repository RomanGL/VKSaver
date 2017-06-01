using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
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
            LoadScrollsPosition();

            var audiosScroll = AudiosListView.GetScrollViewer();
            audiosScroll.ViewChanged += (s, args) =>
            {
                if (VM != null)
                    VM.AudiosScrollPosition = audiosScroll.VerticalOffset;
            };
        }

        private void VideosGridView_Loaded(object sender, RoutedEventArgs e)
        {
            VideosGridView.Loaded -= VideosGridView_Loaded;
            LoadScrollsPosition();

            var videosScroll = VideosGridView.GetScrollViewer();
            videosScroll.ViewChanged += (s, args) =>
            {
                if (VM != null)
                    VM.VideosScrollPosition = videosScroll.VerticalOffset;
            };
        }

        private void DocsListView_Loaded(object sender, RoutedEventArgs e)
        {
            DocsListView.Loaded -= DocsListView_Loaded;
            LoadScrollsPosition();

            var docsScroll = DocsListView.GetScrollViewer();
            docsScroll.ViewChanged += (s, args) =>
            {
                if (VM != null)
                    VM.DocsScrollPosition = docsScroll.VerticalOffset;
            };
        }

        private void LoadScrollsPosition()
        {
            switch (MainPivot.SelectedIndex)
            {
                case 0:
                    AudiosListView.GetScrollViewer()?.ChangeView(null, VM?.AudiosScrollPosition, null, true);
                    break;
                case 1:
                    VideosGridView.GetScrollViewer()?.ChangeView(null, VM?.VideosScrollPosition, null, true);
                    break;
                case 2:
                    DocsListView.GetScrollViewer()?.ChangeView(null, VM?.DocsScrollPosition, null, true);
                    break;
            }
        }
    }
}
