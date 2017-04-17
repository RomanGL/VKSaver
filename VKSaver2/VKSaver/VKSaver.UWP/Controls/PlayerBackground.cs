using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Microsoft.Practices.ServiceLocation;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Controls
{
    public partial class PlayerBackground
    {
        public PlayerBackground()
        {
            InitializeComponent();
            if (!DesignMode.DesignModeEnabled)
            {
                _imagesCacheService = ServiceLocator.Current.GetInstance<IImagesCacheService>();
                this.Loaded += PlayerBackground_Loaded;
                this.Unloaded += PlayerBackground_Unloaded;
            }

            this.SizeChanged += (s, e) =>
            {
                CancelAlbumsGridState();
                CalculateObjectsSize();

                int width = (int)e.NewSize.Width;
                int height = (int)e.NewSize.Height;
                CalculateSizes(width, height);

                RootCanvas.Clip = new RectangleGeometry
                {
                    Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height)
                };
            };
        }

        private void PlayerBackground_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= PlayerBackground_Loaded;
            Start();
        }

        private void PlayerBackground_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= PlayerBackground_Unloaded;
            Stop();
        }
    }
}
