using System;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    public sealed class UserContentGroupKeyToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;

            switch (value.ToString())
            {
                case "albums":
                    return "альбомы";
                case "audios":
                    return "аудиозаписи";
                case "videos":
                    return "видеозаписи";
                default:
                    return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
