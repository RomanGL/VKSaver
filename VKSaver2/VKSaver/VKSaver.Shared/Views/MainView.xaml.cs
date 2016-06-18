using Microsoft.Practices.Prism.StoreApps;
using VKSaver.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Views
{
    /// <summary>
    /// Представление главной страницы.
    /// </summary>
    public sealed partial class MainView : VisualStateAwarePage
    {
        public MainView()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;      
#if WINDOWS_APP
            this.SizeChanged += MainView_SizeChanged;
#endif
        } 

#if WINDOWS_APP
        /// <summary>
        /// Вызывается при нажатии на заголовок хаба.
        /// </summary>
        private void Hub_SectionHeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            switch ((string)e.Section.Tag)
            {
                case "TopTracks":
                    Commands.GoToTopTracksView.Execute(null);
                    break;
                case "TopArtists":
                    Commands.GoToTopArtistsView.Execute(null);
                    break;
            }
        }
        private void MainView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if (e.NewSize.Width < 500)
            //{
            //    VisualStateManager.GoToState(this, "MinimalLayout", true);
            //}
            //else
            //{
            //    VisualStateManager.GoToState(this, "DefaultLayout", true);
            //}
        }
#endif
    }
}
