using System;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    /// <summary>
    /// Представляет конвертер процентных значений.
    /// Конвертирует 0.5 в 50.
    /// </summary>
    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is double))
                throw new ArgumentException("Тип должен быть double.");
            int result = (int)((double)value * 100);

            return result + "%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
