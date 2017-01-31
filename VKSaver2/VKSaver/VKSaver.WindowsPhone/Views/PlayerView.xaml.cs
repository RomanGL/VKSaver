using Microsoft.Practices.Prism.StoreApps;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using VKSaver.Core.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VKSaver.Core.Models;

namespace VKSaver.Views
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class PlayerView : VisualStateAwarePage
    {
        public PlayerView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PlBackground.Start();
            vm = DataContext as PlayerViewModel;
            vm.ShowLikeTrackInfoCommand = new DelegateCommand<AppNotification>(TryShowLikeInfo);
            TracksList.Loaded += TracksList_Loaded;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            PlBackground.Stop();            
            vm = null;

            base.OnNavigatedFrom(e);
        }

        private void TryShowLikeInfo(AppNotification notification)
        {
            LikeInfoNotification.Message = notification;
            LikeInfoNotification.Visibility = Visibility.Visible;

            LikeInfoNotification.Loaded += async (s, e) =>
            {
                LikeInfoNotification.Show();
                await Task.Delay(2000);

                LikeControl.PlayLikeAnimation();
                await Task.Delay(200);
                LikeControl.PlayLikeAnimation();
            };
        }

        private void TracksList_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                TracksList.Loaded -= TracksList_Loaded;
                TracksList.ScrollIntoView(vm.CurrentTrack);
            }
            catch (Exception) { }
        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (vm == null)
                return;

            if (MainPivot.SelectedIndex == 1)
            {
                TracksList.ScrollIntoView(vm.CurrentTrack);
                vm.TrackChanged += TrackChanged;
            }
            else
            {
                vm.TrackChanged -= TrackChanged;
            }
        }

        private void TrackChanged(PlayerViewModel sender, PlayerViewModel.PlayerItem e)
        {
            if (TracksList == null)
                return;
            TracksList.ScrollIntoView(e);
        }

        private PlayerViewModel vm = null;
    }
}
