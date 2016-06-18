using System;
using VKSaver.Core.Models.Common;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    /// <summary>
    /// Представляет конвертер для структуры размера файла.
    /// </summary>
    public class FileSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var size = (FileSize)value;

            if (size.Kilobytes < 1024)
                return String.Format("{0} КБ", size.Kilobytes);
            else if (size.Megabytes >= 1 && size.Megabytes < 1024)
                return String.Format("{0} МБ", Math.Round(size.Megabytes, 2));
            else if (size.Gigabytes >= 1 && size.Gigabytes < 1024)
                return String.Format("{0} ГБ", Math.Round(size.Gigabytes, 2));
            else if (size.Terabytes >= 1 && size.Terabytes < 1024)
                return String.Format("{0} ТБ", Math.Round(size.Terabytes, 2));
            else if (size.Petabytes >= 1 && size.Petabytes < 1024)
                return String.Format("{0} ПБ", Math.Round(size.Petabytes, 2));
            else if (size.Exabytes >= 1 && size.Exabytes < 1024)
                return String.Format("{0} ЭБ", Math.Round(size.Exabytes, 2));
            else
                return String.Format("{0} байт", size.Bytes);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
