using Microsoft.Practices.Prism.StoreApps;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Views
{
    /// <summary>
    /// Представление с текстом трека.
    /// </summary>
    public sealed partial class TrackLyricsView : VisualStateAwarePage
    {
        public TrackLyricsView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            plBackground.Start();
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            plBackground.Stop();
            base.OnNavigatedFrom(e);
        }
    }
}
