using System;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    public sealed class ArtistNameLocalizedConverter : IValueConverter
    {
        public ArtistNameLocalizedConverter()
        {
            _locService = new Lazy<ILocService>(() => ((App)Application.Current).Resolve<ILocService>());
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string name = value.ToString();
            if (name == LibraryDatabaseService.UNKNOWN_ARTIST_NAME)
                return _locService.Value["UnknownArtist_Text"];

            return name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        private readonly Lazy<ILocService> _locService;
    }
}
