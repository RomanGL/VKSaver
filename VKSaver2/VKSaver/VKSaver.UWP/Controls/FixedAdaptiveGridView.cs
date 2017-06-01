using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls;

namespace VKSaver.Controls
{
    public class FixedAdaptiveGridView : AdaptiveGridView
    {
        protected override void PrepareContainerForItemOverride(DependencyObject obj, object item)
        {
            base.PrepareContainerForItemOverride(obj, item);

            var gridViewItem = obj as GridViewItem;
            if (gridViewItem != null && ItemContainerStyleSelector != null)
            {
                gridViewItem.Style = ItemContainerStyleSelector.SelectStyle(item, obj);
            }
        }
    }
}
