using VKSaver.Core.ViewModels.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace VKSaver.Common
{
    public static class MultiSelector
    {
        public static object GetItemsSource(DependencyObject obj)
        {
            return obj.GetValue(ItemsSourceProperty);
        }

        public static void SetItemsSource(DependencyObject obj, object value)
        {
            obj.SetValue(ItemsSourceProperty, value);
        }
        
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.RegisterAttached("ItemsSource", typeof(object), typeof(MultiSelector), 
                new PropertyMetadata(null, OnItemsSourceChanged));

        public static bool GetIsEnabled(Selector target)
        {
            return (bool)target.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(Selector target, bool value)
        {
            target.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(MultiSelector),
                new PropertyMetadata(false, OnIsEnabledChanged));

        private static void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var selector = sender as Selector;

            IMultiSelectCollectionView collectionView = TryGetMultiSelectCollectionView(selector.ItemsSource);
            if (selector != null && collectionView != null)
            {
                if ((bool)e.NewValue)
                    collectionView.AddControl(selector);
                else
                    collectionView.RemoveControl(selector);
            }
        }

        private static void OnItemsSourceChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var selector = sender as Selector;
            selector.ItemsSource = e.NewValue;

            if (GetIsEnabled(selector))
            {
                IMultiSelectCollectionView oldCollectionView = TryGetMultiSelectCollectionView(e.OldValue);
                IMultiSelectCollectionView newCollectionView = TryGetMultiSelectCollectionView(e.NewValue);

                if (oldCollectionView != null)
                    oldCollectionView.RemoveControl(selector);

                if (newCollectionView != null)
                    newCollectionView.AddControl(selector);
            }
        }

        private static IMultiSelectCollectionView TryGetMultiSelectCollectionView(object obj)
        {
            IMultiSelectCollectionView collectionView = obj as IMultiSelectCollectionView;

            return collectionView;
        }
    }
}
