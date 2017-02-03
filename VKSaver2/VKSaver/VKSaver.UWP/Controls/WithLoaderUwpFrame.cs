using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Controls
{
    public sealed class WithLoaderUwpFrame : Frame
    {
        public WithLoaderUwpFrame()
        {
            this.DefaultStyleKey = typeof(WithLoaderUwpFrame);
            SurfaceLoader.Initialize(ElementCompositionPreview.GetElementVisual(this).Compositor);
        }
    }
}
