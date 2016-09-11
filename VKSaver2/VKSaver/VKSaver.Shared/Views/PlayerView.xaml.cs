using Microsoft.Practices.Prism.StoreApps;
using System;
using VKSaver.Core.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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
            TracksList.Loaded += TracksList_Loaded;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            PlBackground.Stop();            
            vm = null;

            base.OnNavigatedFrom(e);
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
