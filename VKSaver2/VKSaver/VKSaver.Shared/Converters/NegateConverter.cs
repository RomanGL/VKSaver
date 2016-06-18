using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    /// <summary>
    /// Конвертирует числовые значения в обратные.
    /// </summary>
    public class NegateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {            
            if (value is double)
                return -(double)value;
            if (value is int)
                return -(int)value;
            if (value is long)
                return -(long)value;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
