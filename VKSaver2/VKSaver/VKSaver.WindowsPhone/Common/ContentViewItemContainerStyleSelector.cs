using ModernDev.InTouch;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VKSaver.Common
{
    public sealed class ContentViewItemContainerStyleSelector : StyleSelector
    {
        public Style DefaultStyle { get; set; }
        public Style AlbumStyle { get; set; }
        
        protected override Style SelectStyleCore(object item, DependencyObject container)
        {
            if (item is AudioAlbum || item is VideoAlbum)
                return AlbumStyle;
            return DefaultStyle;
        }
    }
}
