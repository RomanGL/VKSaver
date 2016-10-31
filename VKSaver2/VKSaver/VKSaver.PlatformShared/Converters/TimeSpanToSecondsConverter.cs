using System;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    public sealed class TimeSpanToSecondsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var time = (TimeSpan)value;
            return time.TotalSeconds;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return TimeSpan.FromSeconds((double)value);
        }
    }
}
