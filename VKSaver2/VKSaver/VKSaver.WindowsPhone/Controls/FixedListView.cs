using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VKSaver.Controls
{
    public class FixedListView : ListView
    {
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (ItemContainerStyleSelector != null)
            {
                var fElement = element as FrameworkElement;
                if (fElement == null)
                    return;

                fElement.Style = ItemContainerStyleSelector.SelectStyle(item, null);
            }                
        }
    }
}
