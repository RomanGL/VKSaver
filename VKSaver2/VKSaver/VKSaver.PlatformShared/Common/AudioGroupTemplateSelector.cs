using ModernDev.InTouch;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VKSaver.Common
{
    public sealed class AudioGroupTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AudioTemplate { get; set; }
        public DataTemplate PlaylistTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is Audio)
                return AudioTemplate;
            if (item is Playlist)
                return PlaylistTemplate;

            return null;
        }
    }
}
