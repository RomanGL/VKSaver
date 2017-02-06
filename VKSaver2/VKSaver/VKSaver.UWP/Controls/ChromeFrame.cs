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
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace VKSaver.Controls
{
    public sealed class ChromeFrame : Frame, IAppNotificationsPresenter
    {
        private UIElement _topBar;
        private Grid _menuBar;
        private UIElement _dismissLayer;
        private Rectangle _translatePanel;

        private CompositeTransform _menuBarTransform;

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
            _menuBar = GetTemplateChild("MenuBar") as Grid;
            _dismissLayer = GetTemplateChild("DismissLayer") as UIElement;
            _translatePanel = GetTemplateChild("TranslatePanel") as Rectangle;

            if (!DesignMode.DesignModeEnabled)
            {
                _menuBarTransform = new CompositeTransform();
                _menuBar.RenderTransform = _menuBarTransform;
                _menuBarTransform.TranslateX = -_menuBar.Width;

                UpdateTitleBar();
                SubsribePaneEvents();
            }
        }

        private void UpdateTitleBar()
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(_topBar);
        }

        private void SubsribePaneEvents()
        {
            _menuBar.ManipulationDelta += _menuBar_ManipulationDelta;
            _translatePanel.ManipulationDelta += _translatePanel_ManipulationDelta;
        }

        private void _menuBar_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            DeltaMenuBar(e);
        }

        private void _translatePanel_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            DeltaMenuBar(e);
        }

        private void DeltaMenuBar(ManipulationDeltaRoutedEventArgs e)
        {
            _menuBarTransform.TranslateX += e.Delta.Translation.X;
        }
    }
}
