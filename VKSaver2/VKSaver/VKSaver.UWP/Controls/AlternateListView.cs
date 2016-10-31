using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace VKSaver.Controls
{
    /// <summary>
    /// Представляет <see cref="ListView"/> с выделением четных и нечетных элементов.
    /// </summary>
    public class AlternateListView : ListView
    {
        public SolidColorBrush FirstItemBackground
        {
            get { return (SolidColorBrush)GetValue(FirstItemBackgroundProperty); }
            set { SetValue(FirstItemBackgroundProperty, value); }
        }
        
        public static readonly DependencyProperty FirstItemBackgroundProperty =
            DependencyProperty.Register("FirstItemBackground", typeof(SolidColorBrush), 
                typeof(AlternateListView), new PropertyMetadata(default(SolidColorBrush)));
        
        public SolidColorBrush SecondItemBackground
        {
            get { return (SolidColorBrush)GetValue(SecondItemBackgroundProperty); }
            set { SetValue(SecondItemBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SecondItemBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondItemBackgroundProperty =
            DependencyProperty.Register("SecondItemBackground", typeof(SolidColorBrush), 
                typeof(AlternateListView), new PropertyMetadata(default(SolidColorBrush)));

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            int index = IndexFromContainer(element);
            ListViewItem lvi = element as ListViewItem;
            if (index % 2 != 0)
            {
                if (FirstItemBackground != null)
                    lvi.Background = FirstItemBackground;
            }
            else
            {
                if (SecondItemBackground != null)
                    lvi.Background = SecondItemBackground;
            }
        }
    }
}
