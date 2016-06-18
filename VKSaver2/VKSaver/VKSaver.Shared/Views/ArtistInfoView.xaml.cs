using Microsoft.Practices.ServiceLocation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VKSaver.Core;
using System.Collections.Generic;
using Microsoft.Practices.Prism.StoreApps;

namespace VKSaver.Views
{
    /// <summary>
    /// Представляет страницу с информацией об исполнителе.
    /// </summary>
    public sealed partial class ArtistInfoView : VisualStateAwarePage
    {
#if WINDOWS_APP
        /// <summary>
        /// Помощник навигации.
        /// </summary>
        public VKSaver.Helpers.WindowsNavigationHelper WindowsNavigationHelper { get; private set; }
#endif        

        public ArtistInfoView()
        {
            this.InitializeComponent();
#if WINDOWS_APP
            WindowsNavigationHelper = new VKSaver.Helpers.WindowsNavigationHelper(this);
#endif
        }
    }
}
