using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    /// <summary>
    /// Конвертер, возвращающий ширину экрана.
    /// </summary>
    public class ScreenWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double val = 0;
            if (parameter != null) val = double.Parse((string)parameter);
            return Window.Current.Bounds.Width + val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
