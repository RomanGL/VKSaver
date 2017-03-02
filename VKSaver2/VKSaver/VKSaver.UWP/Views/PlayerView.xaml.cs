using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Prism.Windows.Mvvm;
using VKSaver.Common;
using VKSaver.Core.ViewModels;

namespace VKSaver.Views
{
    public sealed partial class PlayerView : SessionStateAwarePage
    {
        public PlayerView()
        {
            this.InitializeComponent();
            this.Loaded += PlayerView_Loaded;
        }

        private void PlayerView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= PlayerView_Loaded;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            plBackground.Start();
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            WindowThemeHelper.HideTitleBar();
            _isTracksVisible = true;

            base.OnNavigatedTo(navigationEventArgs);

            var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("TrackBlock");
            animation?.TryStart(TrackBlock);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            plBackground.Stop();

            //ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("TrackBlock", TrackBlock);

            base.OnNavigatingFrom(e);
        }

        private bool _isTracksVisible;
        private void ShowTrackButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_isTracksVisible)
                HideTracksStoryboard.Begin();
            else
                ShowTracksStoryboard.Begin();

            _isTracksVisible = !_isTracksVisible;
        }
    }
}
