using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    /// <summary>
    /// Конвертирует данные события ItemClick в данные нажатого элента.
    /// </summary>
    public class HamburgerItemClickConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((HamburgerMenuGlyphItem)((ItemClickEventArgs)value).ClickedItem).Tag.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
