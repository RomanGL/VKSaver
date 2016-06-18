using OneTeam.SDK.VK.Models.Video;
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
            if (item is VKVideo)
                return VideoTemplate;
            else if (item is VKVideoAlbum)
                return AlbumTemplate;
            else
                return null;
        }
    }
}
