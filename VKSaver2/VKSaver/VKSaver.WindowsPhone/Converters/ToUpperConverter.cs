using System;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    /// <summary>
    /// Класс-конвертер. Преобразовывает строку в заглавный регистр.
    /// </summary>
    public class ToUpperConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is String))
                return "CONVERSION ERROR";
            return ((String)value).ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
