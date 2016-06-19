using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    public sealed class BoolToListViewSelectionModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool mode = (bool)value;
            if (mode)
                return ListViewSelectionMode.Multiple;
            return ListViewSelectionMode.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
