using System;
using VKSaver.Core.Services.Interfaces;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    public sealed class LaunchViewNameConverter : IValueConverter
    {
        public LaunchViewNameConverter()
        {
            _locService = new Lazy<ILocService>(() => ((App)Application.Current).Resolve<ILocService>());
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch (value.ToString())
            {
                case "MainView":
                    return _locService.Value["LaunchView_MainView_Name"];
                case "SpecialMainView":
                    return _locService.Value["LaunchView_MainView_Name"];
                case "UserContentView":
                    return _locService.Value["LaunchView_UserContentView_Name"];
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        private readonly Lazy<ILocService> _locService;
    }
}
