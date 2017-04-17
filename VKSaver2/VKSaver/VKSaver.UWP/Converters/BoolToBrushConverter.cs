using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace VKSaver.Converters
{
    public sealed class BoolToBrushConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty TrueBrushProperty = DependencyProperty.Register(
            "TrueBrush", typeof(Brush), typeof(BoolToBrushConverter), new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty FalseBrushProperty = DependencyProperty.Register(
            "FalseBrush", typeof(Brush), typeof(BoolToBrushConverter), new PropertyMetadata(default(Brush)));

        public Brush TrueBrush
        {
            get { return (Brush) GetValue(TrueBrushProperty); }
            set { SetValue(TrueBrushProperty, value); }
        }

        public Brush FalseBrush
        {
            get { return (Brush) GetValue(FalseBrushProperty); }
            set { SetValue(FalseBrushProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool flag = (bool) value;
            if (flag)
                return TrueBrush;
            else
                return FalseBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
