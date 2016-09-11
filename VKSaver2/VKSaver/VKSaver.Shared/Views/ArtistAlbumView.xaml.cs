using Microsoft.Practices.ServiceLocation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VKSaver.Core;
using System.Collections.Generic;
using Microsoft.Practices.Prism.StoreApps;

namespace VKSaver.Views
{
    /// <summary>
    /// Представляет страницу с информацией об альбоме исполнителя.
    /// </summary>
    public sealed partial class ArtistAlbumView : VisualStateAwarePage
    {
#if WINDOWS_APP
        /// <summary>
        /// Помощник навигации.
        /// </summary>
        public VKSaver.Helpers.WindowsNavigationHelper WindowsNavigationHelper { get; private set; }
#endif        

        public ArtistAlbumView()
        {
            this.InitializeComponent();
#if WINDOWS_APP
            WindowsNavigationHelper = new VKSaver.Helpers.WindowsNavigationHelper(this);
#endif
        }
    }
}
