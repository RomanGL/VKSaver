using Microsoft.Practices.ServiceLocation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VKSaver.Core;
using System.Collections.Generic;
using Microsoft.Practices.Prism.StoreApps;

namespace VKSaver.Views
{
    public sealed partial class NewsMediaView : VisualStateAwarePage
    {
#if WINDOWS_APP
        /// <summary>
        /// Помощник навигации.
        /// </summary>
        public VKSaver.Helpers.WindowsNavigationHelper WindowsNavigationHelper { get; private set; }
#endif        

        public NewsMediaView()
        {
            this.InitializeComponent();
#if WINDOWS_APP
            WindowsNavigationHelper = new VKSaver.Helpers.WindowsNavigationHelper(this);
#endif
        }
    }
}
