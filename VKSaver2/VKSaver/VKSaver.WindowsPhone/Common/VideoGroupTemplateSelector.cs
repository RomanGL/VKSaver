using ModernDev.InTouch;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VKSaver.Common
{
    public sealed class VideoGroupTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AlbumTemplate { get; set; }

        public DataTemplate VideoTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is Video)
                return VideoTemplate;
            else if (item is VideoAlbum)
                return AlbumTemplate;
            else
                return null;
        }
    }
}
