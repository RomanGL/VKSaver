using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using System.Linq;
using Windows.ApplicationModel;

namespace VKSaver.Converters
{
    /// <summary>
    /// Конвертер размера дя элементов, размещенных в списке по одному по всей ширине.
    /// </summary>
    public class OneItemsListSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
#if DEBUG
            if (DesignMode.DesignModeEnabled)
                return 350;
#endif
            if (parameter != null)
            {
                int[] margins = parameter.ToString().Split(new char[] { ',' }).Select(s => int.Parse(s)).ToArray();
                return Window.Current.Bounds.Width - margins[0] - margins[1];
            }

            return Window.Current.Bounds.Width - 38;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
