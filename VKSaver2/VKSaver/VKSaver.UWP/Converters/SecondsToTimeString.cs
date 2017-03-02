using System;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    public sealed class SecondsToTimeString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double seconds = (double)value;
            return TimeSpan.FromSeconds(seconds).ToString(@"mm\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
