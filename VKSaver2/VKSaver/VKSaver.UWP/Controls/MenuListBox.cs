using System.Linq;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VKSaver.Common;

namespace VKSaver.Controls
{
    public sealed class MenuListBox : ListBox
    {
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            int index = IndexFromContainer(element);
            var lvi = element as ListBoxItem;
            if (index == 0)
            {
                lvi.Loaded += Lvi_Loaded;
            }
        }

        private void Lvi_Loaded(object sender, RoutedEventArgs e)
        {
            var lvi = (ListBoxItem) sender;
            lvi.Loaded -= Lvi_Loaded;

            var tbs = lvi.GetDescendantsOfType<TextBlock>().ToList();
            tbs[1].FontWeight = FontWeights.Bold;
        }
    }
}
