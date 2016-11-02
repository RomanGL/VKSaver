using Prism.Windows.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VKSaver.Core.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Views
{
    public sealed partial class PlayerView : SessionStateAwarePage
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

        private void TracksList_Loaded(object sender, RoutedEventArgs e)
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
