using Windows.ApplicationModel;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Microsoft.Practices.ServiceLocation;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Controls
{
    public partial class PlayerBackground
    {
        static PlayerBackground()
        {
#if DEBUG
            int screenHeight;
            int screenWidth;
            if (DesignMode.DesignModeEnabled)
            {
                screenHeight = 384;
                screenWidth = 640;
            }
            else
            {
                screenHeight = (int)Window.Current.Bounds.Height;
                screenWidth = (int)Window.Current.Bounds.Width;

#if WINDOWS_PHONE_APP
                UpdateSizeWithStatusBar(ref screenWidth, ref screenHeight);
#endif
            }
#else
            int screenHeight = (int)Window.Current.CoreWindow.Bounds.Height;
            int screenWidth = (int)Window.Current.CoreWindow.Bounds.Width;
#endif
            Window.Current.SizeChanged += (s, e) =>
            {
                int width = (int)e.Size.Width;
                int height = (int)e.Size.Height;
#if WINDOWS_PHONE_APP && DEBUG
                if (!DesignMode.DesignModeEnabled)
                    UpdateSizeWithStatusBar(ref width, ref height);
#endif
                CalculateSizes(width, height);
            };

            CalculateSizes(screenWidth, screenHeight);
        }

#if WINDOWS_PHONE_APP
        private static void UpdateSizeWithStatusBar(ref int width, ref int height)
        {
            var statusbar = StatusBar.GetForCurrentView();
            if (statusbar.OccludedRect.Height > statusbar.OccludedRect.Width)
                width += (int)statusbar.OccludedRect.Width;
            else
                height += (int)statusbar.OccludedRect.Height;
        }

        private void UpdateMargins()
        {
            var statusbar = StatusBar.GetForCurrentView();
            if (statusbar.OccludedRect.Height > statusbar.OccludedRect.Width)
                this.Margin = new Thickness(-statusbar.OccludedRect.Width, 0, 0, 0);
            else
                this.Margin = new Thickness(0, -statusbar.OccludedRect.Height, 0, 0);
        }
#endif

        /// <summary>
        /// Инициализирует новый экземпляр класса.
        /// </summary>
        public PlayerBackground()
        {
            InitializeComponent();
            if (!DesignMode.DesignModeEnabled)
            {
#if WINDOWS_PHONE_APP
                UpdateMargins();
#endif
                _imagesCacheService = ServiceLocator.Current.GetInstance<IImagesCacheService>();
            }

            this.SizeChanged += (s, e) =>
            {
                CancelAlbumsGridState();
                CalculateObjectsSize();

                int width = (int)e.NewSize.Width;
                int height = (int)e.NewSize.Height;
                CalculateSizes(width, height);
            };
        }
    }
}
