using System;
using Windows.UI.Xaml.Data;
using Windows.ApplicationModel;

namespace VKSaver.Converters
{
    /// <summary>
    /// Конвертертер половинного размера. Возвращает половину от полученной величины.
    /// </summary>
    public class HalfSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
#if DEBUG
            if (DesignMode.DesignModeEnabled)
                return 175;
#endif
            double val = (double)value;
            return val / 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
