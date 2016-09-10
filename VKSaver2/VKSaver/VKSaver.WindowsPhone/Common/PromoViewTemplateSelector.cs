using VKSaver.Core.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VKSaver.Common
{
    public sealed class PurchaseViewTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CommonTemplate { get; set; }
        public DataTemplate PurchaseTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var view = item as PurchaseViewModel.ScreenItem;
            if (view.IsPurchaseItem)
                return PurchaseTemplate;
            return CommonTemplate;
        }
    }
}
