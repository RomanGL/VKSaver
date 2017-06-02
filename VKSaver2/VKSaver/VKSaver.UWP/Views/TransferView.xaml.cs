using Windows.UI.Xaml;
using Prism.Windows.Mvvm;
using VKSaver.Common;
using VKSaver.Core.ViewModels;

namespace VKSaver.Views
{
    public sealed partial class TransferView : SessionStateAwarePage
    {
        public TransferView()
        {
            this.InitializeComponent();
            this.Loaded += TransferView_Loaded;
            DownloadsListView.Loaded += DownloadsListView_Loaded;
            UploadsListView.Loaded += UploadsListView_Loaded;
        }

        private TransferViewModel VM => this.DataContext as TransferViewModel;

        private void TransferView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= TransferView_Loaded;
            FadeInAnimation.Begin();
        }

        private void DownloadsListView_Loaded(object sender, RoutedEventArgs e)
        {
            DownloadsListView.Loaded -= DownloadsListView_Loaded;

            var downloadsScroll = DownloadsListView.GetScrollViewer();
            downloadsScroll.ChangeView(null, VM?.DownloadsScrollPosition, null, true);
            downloadsScroll.ViewChanged += (s, args) =>
            {
                if (VM != null)
                    VM.DownloadsScrollPosition = downloadsScroll.VerticalOffset;
            };
        }

        private void UploadsListView_Loaded(object sender, RoutedEventArgs e)
        {
            UploadsListView.Loaded -= UploadsListView_Loaded;

            var uploadsScroll = UploadsListView.GetScrollViewer();
            uploadsScroll.ChangeView(null, VM?.UploadsScrollPosition, null, true);
            uploadsScroll.ViewChanged += (s, args) =>
            {
                if (VM != null)
                    VM.UploadsScrollPosition = uploadsScroll.VerticalOffset;
            };
        }
    }
}
