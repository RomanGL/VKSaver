using System;
using VKSaver.Common;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    public sealed class AuthServiceNameLocConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string serviceName = value.ToString();
            switch (serviceName)
            {
                case "vk.com":
                    return _locWrapper["ServiceName_VK_Text"];
                case "last.fm":
                    return _locWrapper["ServiceName_LastFm_Text"];
                default:
                    return serviceName;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        private readonly LocalizationXamlWrapper _locWrapper = new LocalizationXamlWrapper();
    }
}
