using System;
using Windows.UI.Text;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace VKSaver.Converters
{
    public sealed class CurrentTrackToStyleConverter : IValueConverter
    {
        public Brush NormalBrush { get; set; }
        public Brush CurrentBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var isCurrent = (bool)value;

            if (targetType == typeof(Brush))
                return isCurrent ? CurrentBrush : NormalBrush;
            else if (targetType == typeof(FontWeight))
                return isCurrent ? FontWeights.SemiBold : FontWeights.SemiLight;
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
