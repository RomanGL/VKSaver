using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;

namespace VKSaver.Converters
{
    /// <summary>
    /// Конвертериет bool в значение видимости.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool reverse = false;
            if (parameter != null)
                bool.TryParse(parameter.ToString(), out reverse);

            if ((bool)value)
                return reverse ? Visibility.Collapsed : Visibility.Visible;
            else
                return reverse ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
