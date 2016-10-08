using System;
using VKSaver.Core.Models.Player;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    public sealed class PlayerRepeatModeToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (int)(PlayerRepeatMode)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (PlayerRepeatMode)(int)value;
        }
    }
}
