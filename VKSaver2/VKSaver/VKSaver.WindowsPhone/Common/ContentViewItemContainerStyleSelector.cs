using OneTeam.SDK.VK.Models.Audio;
using OneTeam.SDK.VK.Models.Video;
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
            if (item is VKAudioAlbum || item is VKVideoAlbum)
                return AlbumStyle;
            return DefaultStyle;
        }
    }
}
