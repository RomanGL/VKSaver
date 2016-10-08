using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    /// <summary>
    /// Конвертер, возвращающий высоту экрана.
    /// </summary>
    public class ScreenHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Window.Current.Bounds.Height;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
