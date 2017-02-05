using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using VKSaver.Core.Models;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Controls
{
    public sealed class ChromeFrame : Frame, IAppNotificationsPresenter
    {
        private UIElement _topBar;

        public ChromeFrame()
        {
            this.DefaultStyleKey = typeof(ChromeFrame);
            SurfaceLoader.Initialize(ElementCompositionPreview.GetElementVisual(this).Compositor);
        }

        public void ShowNotification(AppNotification notification)
        {
            Debug.WriteLine($"App notification: {notification.Title} - {notification.Content}\nDuration {notification.Duration}");
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _topBar = GetTemplateChild("TopBar") as UIElement;

            if (!DesignMode.DesignModeEnabled)
            {
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                Window.Current.SetTitleBar(_topBar);
            }
        }
    }
}
