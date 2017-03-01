using System;
using System.Linq;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using ModernDev.InTouch;
using Prism.Windows.Mvvm;
using VKSaver.Core.ViewModels;

namespace VKSaver.Views
{
    public sealed partial class MainView : SessionStateAwarePage
    {
        //private Compositor _compositor;
        private Audio _navigatedTrack;

        public MainView()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;

            PlayerTracksList.ItemClick += PlayerTracksList_ItemClick;
            PlayerTracksList.Loaded += PlayerTracksList_Loaded;

            //_compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            //var connectedAnimationService = ConnectedAnimationService.GetForCurrentView();
            //connectedAnimationService.DefaultDuration = TimeSpan.FromSeconds(0.8);
            //connectedAnimationService.DefaultEasingFunction = _compositor.CreateCubicBezierEasingFunction(
            //    new Vector2(0.41f, 0.52f),
            //    new Vector2(0.00f, 0.94f)
            //    );
        }

        private void PlayerTracksList_Loaded(object sender, RoutedEventArgs e)
        {
            //var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("Track");
            //if (animation != null)
            //{
            //    var item = ((MainViewModel)this.DataContext).UserTracks.FirstOrDefault(t => t.Id == _navigatedTrack.Id);
            //    if (item == null)
            //    {
            //        animation.Cancel();
            //    }
            //    else
            //    {
            //        PlayerTracksList.ScrollIntoView(item, ScrollIntoViewAlignment.Default);
            //        PlayerTracksList.UpdateLayout();

            //        var container = PlayerTracksList.ContainerFromItem(item) as ListViewItem;
            //        if (container != null)
            //        {
            //            var root = (FrameworkElement)container.ContentTemplateRoot;
            //            animation.TryStart(root);
            //        }
            //        else
            //        {
            //            animation.Cancel();
            //        }
            //    }
            //}

            //_navigatedTrack = null;
        }

        private void PlayerTracksList_ItemClick(object sender, ItemClickEventArgs e)
        {
            //var container = PlayerTracksList.ContainerFromItem(e.ClickedItem) as ListViewItem;
            //if (container != null)
            //{
            //    var root = (FrameworkElement)container.ContentTemplateRoot;
            //    ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("Track", root);
            //    _navigatedTrack = (Audio)e.ClickedItem;
            //}
        }
    }
}
