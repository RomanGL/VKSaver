using VKSaver.Core.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VKSaver.Common
{
    public sealed class PromoViewTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CommonTemplate { get; set; }
        public DataTemplate LetsGoTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var view = item as PromoViewModel.ScreenItem;
            if (view.IsLetsGoItem)
                return LetsGoTemplate;
            return CommonTemplate;
        }
    }
}
