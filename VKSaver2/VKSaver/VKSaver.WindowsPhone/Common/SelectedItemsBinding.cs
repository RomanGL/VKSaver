using System.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VKSaver.Common
{
    public static class SelectedItemsBinding
    {
        public static IList GetSelectedItems(DependencyObject obj)
        {
            return (IList)obj.GetValue(SelectedItemsProperty);
        }

        public static void SetSelectedItems(DependencyObject obj, IList value)
        {
            obj.SetValue(SelectedItemsProperty, value);
        }
        
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached("SelectedItems", typeof(IList), 
                typeof(SelectedItemsBinding), new PropertyMetadata(null, OnSelectedItemsChanged));

        private static bool GetIsSubscribed(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSubscribedProperty);
        }

        private static void SetIsSubscribed(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSubscribedProperty, value);
        }
        
        private static readonly DependencyProperty IsSubscribedProperty =
            DependencyProperty.RegisterAttached("IsSubscribed", typeof(bool), 
                typeof(SelectedItemsBinding), new PropertyMetadata(false));

        private static void OnSelectedItemsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var listView = obj as ListViewBase;

            if (listView != null)
            {
                if (GetIsSubscribed(listView))
                {
                    listView.SelectionChanged -= ListView_SelectionChanged;
                    listView.Unloaded -= ListView_Unloaded;
                    SetIsSubscribed(listView, false);
                }
                else
                {
                    listView.SelectionChanged += ListView_SelectionChanged;
                    listView.Unloaded += ListView_Unloaded;
                    SetIsSubscribed(listView, true);
                }
            }
            else
                return;

            var list = e.NewValue as IList;
            if (list != null)
            {
                list.Clear();
                foreach (var item in listView.SelectedItems)
                    list.Add(item);
            }            
        }

        private static void ListView_Unloaded(object sender, RoutedEventArgs e)
        {
            var listView = (ListViewBase)sender;
            SetIsSubscribed(listView, false);
            listView.Unloaded -= ListView_Unloaded;
            listView.SelectionChanged -= ListView_SelectionChanged;
        }

        private static void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = GetSelectedItems((ListViewBase)sender);
            if (list == null)
                return;

            foreach (var item in e.RemovedItems)
                list.Remove(item);

            foreach (var item in e.AddedItems)
                list.Add(item);
        }
    }
}
