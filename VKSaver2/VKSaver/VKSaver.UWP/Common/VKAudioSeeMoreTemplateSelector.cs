using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using ModernDev.InTouch;
using VKSaver.Core.ViewModels;

namespace VKSaver.Common
{
    public sealed class VKAudioSeeMoreTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AudioTemplate { get; set; }
        public DataTemplate SeeAlsoTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var audio = item as Audio;
            if (audio == null)
                return null;

            if (audio.Title == MainViewModel.VKSAVER_SEE_ALSO_TEXT)
                return SeeAlsoTemplate;
            return AudioTemplate;
        }
    }
}
