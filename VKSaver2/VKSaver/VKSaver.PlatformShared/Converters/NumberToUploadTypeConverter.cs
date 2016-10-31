using System;
using VKSaver.Core.ViewModels.Common;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    public sealed class NumberToUploadTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (int)(UploadType)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (UploadType)(int)value;
        }
    }
}
