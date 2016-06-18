using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;
using Windows.ApplicationModel;

namespace VKSaver.Converters
{
    /// <summary>
    /// Конвертер размера для страницы списка с двумя элементами по ширине.
    /// </summary>
    public class TwoItemsListSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
#if DEBUG
            if (DesignMode.DesignModeEnabled)
                return 150;
#endif
            return (Window.Current.Bounds.Width - 53) / 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
