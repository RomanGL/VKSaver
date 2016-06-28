using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    /// <summary>
    /// Конвертирует отрезок времени в его строковое представление.
    /// </summary>
    public class TimeSpanToStringConverter : IValueConverter
    {
        /// <summary>
        /// Конверитирует System.TimeSpan в его строковое представление.
        /// Поддерживает инверсию.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            TimeSpan time;

            if (value == null)
                throw new ArgumentNullException("value");
            if (value is int)
                time = TimeSpan.FromSeconds((int)value);
            else if (value.GetType() == typeof(TimeSpan))
                time = (TimeSpan)value;
            else
                throw new ArgumentException("targetType", "Ожидался тип System.TimeSpan.");

            bool isNegative = parameter == null ? 
                false : bool.Parse(parameter.ToString());            

            try
            {
                if (time.Hours == 0)
                {
                    if (time.Minutes < 10)
                        return isNegative ?
                            time.ToString(@"\-m\:ss") :
                            time.ToString(@"m\:ss");
                    else
                        return isNegative ?
                            time.ToString(@"\-mm\:ss") :
                            time.ToString(@"mm\:ss");
                }
                else
                {
                    {
                        if (time.Minutes < 10)
                            return isNegative ?
                                time.ToString(@"\-h\:m\:ss") :
                                time.ToString(@"h\:m\:ss");
                        else
                            return isNegative ?
                                time.ToString(@"\-h\:mm\:ss") :
                                time.ToString(@"h:\mm\:ss");
                    }
                }
            }
            catch (FormatException)
            {
                return "∞:∞∞";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
