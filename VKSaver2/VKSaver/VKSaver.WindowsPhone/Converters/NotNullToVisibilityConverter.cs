using System;
using System.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    public sealed class NotNullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool reverse = false;
            if (parameter != null)
                bool.TryParse(parameter.ToString(), out reverse);

            if (value is String)
            {
                if (String.IsNullOrEmpty(value.ToString()))
                    return reverse ? Visibility.Visible : Visibility.Collapsed;
                else
                    return reverse ? Visibility.Collapsed : Visibility.Visible;
            }
            else if (value is IList)
            {
                if (((IList)value).Count == 0)
                    return reverse ? Visibility.Visible : Visibility.Collapsed;
                else
                    return reverse ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                if (value == null)
                    return reverse ? Visibility.Visible : Visibility.Collapsed;
                else
                    return reverse ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
