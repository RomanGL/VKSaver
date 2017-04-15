using ModernDev.InTouch;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VKSaver.Common
{
    public sealed class AudioGroupTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AlbumTemplate { get; set; }
        public DataTemplate AudioTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is Audio)
                return AudioTemplate;
            else if (item is AudioAlbum)
                return AlbumTemplate;
            else
                return null;
        }
    }
}
