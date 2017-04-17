using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using IF.Lastfm.Core.Objects;
using VKSaver.Core.ViewModels;

namespace VKSaver.Common
{
    public sealed class LastArtistSeeMoreTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ArtistTemplate { get; set; }
        public DataTemplate SeeAlsoTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var artist = item as LastArtist;
            if (artist == null)
                return null;

            if (artist.Name == MainViewModel.VKSAVER_SEE_ALSO_TEXT)
                return SeeAlsoTemplate;
            return ArtistTemplate;
        }
    }
}
