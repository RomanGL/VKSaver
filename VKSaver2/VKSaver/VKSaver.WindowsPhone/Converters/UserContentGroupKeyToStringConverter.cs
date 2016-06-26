using Microsoft.Practices.ServiceLocation;
using System;
using VKSaver.Core.Services.Interfaces;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    public sealed class UserContentGroupKeyToStringConverter : IValueConverter
    {
        public UserContentGroupKeyToStringConverter()
        {
            _locService = ServiceLocator.Current.GetInstance<ILocService>();
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;

            switch (value.ToString())
            {
                case "albums":
                    return _locService["UserContentView_AlbumsGroup_Text"];
                case "audios":
                    return _locService["UserContentView_AudiosGroup_Text"];
                case "videos":
                    return _locService["UserContentView_VideosGroup_Text"];
                default:
                    return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        private readonly ILocService _locService;
    }
}
