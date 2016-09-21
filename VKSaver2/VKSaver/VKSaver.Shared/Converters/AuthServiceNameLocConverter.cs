using System;
using VKSaver.Core.Services.Interfaces;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    public sealed class AuthServiceNameLocConverter : IValueConverter
    {
        public AuthServiceNameLocConverter()
        {
            _locService = new Lazy<ILocService>(() => ((App)Application.Current).Resolve<ILocService>());
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string serviceName = value.ToString();
            switch (serviceName)
            {
                case "vk.com":
                    return _locService.Value["ServiceName_VK_Text"];
                case "last.fm":
                    return _locService.Value["ServiceName_LastFm_Text"];
                default:
                    return serviceName;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        private readonly Lazy<ILocService> _locService;
    }
}
