using OneTeam.SDK.VK.Models.Audio;
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
            if (item is VKAudio)
                return AudioTemplate;
            else if (item is VKAudioAlbum)
                return AlbumTemplate;
            else
                return null;
        }
    }
}
