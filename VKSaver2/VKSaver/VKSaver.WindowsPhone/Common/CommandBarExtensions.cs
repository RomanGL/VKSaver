using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VKSaver.Common
{
    public static class CommandBarExtensions
    {
        public static ObservableCollection<ICommandBarElement> GetCommandBarItems(DependencyObject obj)
        {
            return (ObservableCollection<ICommandBarElement>)obj.GetValue(CommandBarItemsProperty);
        }

        public static void SetCommandBarItems(DependencyObject obj, ObservableCollection<ICommandBarElement> value)
        {
            obj.SetValue(CommandBarItemsProperty, value);
        }
        
        public static readonly DependencyProperty CommandBarItemsProperty =
            DependencyProperty.RegisterAttached("CommandBarItems", typeof(ObservableCollection<ICommandBarElement>), 
                typeof(CommandBarExtensions), new PropertyMetadata(null, OnCommandBarItemsChanged));

        private static void OnCommandBarItemsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var bar = obj as CommandBar;
            if (bar == null)
                return;

            if (e.OldValue != null)
            {
                Unsubscribe(bar, e.OldValue as ObservableCollection<ICommandBarElement>);
                bar.PrimaryCommands.Clear();
            }
            if (e.NewValue != null)
            {
                var collection = e.NewValue as ObservableCollection<ICommandBarElement>;

                foreach (var item in collection)
                    bar.PrimaryCommands.Add(item);

                Subscribe(bar, collection);                
            }
        }

        private static void Subscribe(CommandBar bar, ObservableCollection<ICommandBarElement> collection)
        {
            bar.Unloaded += Bar_Unloaded;
            collection.CollectionChanged += Collection_CollectionChanged;
            _bindings[collection] = bar;
        }        

        private static void Unsubscribe(CommandBar bar, ObservableCollection<ICommandBarElement> collection)
        {
            collection.CollectionChanged -= Collection_CollectionChanged;
            _bindings.Remove(collection);
        }

        private static void Bar_Unloaded(object sender, RoutedEventArgs e)
        {
            var bar = sender as CommandBar;
            bar.Unloaded -= Bar_Unloaded;
            SetCommandBarItems(bar, null);
        }

        private static void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var bar = _bindings[sender as ObservableCollection<ICommandBarElement>];
            if (bar == null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                        bar.PrimaryCommands.Add(item as ICommandBarElement);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                        bar.PrimaryCommands.Remove(item as ICommandBarElement);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        int index = bar.PrimaryCommands.IndexOf(e.OldItems[i] as ICommandBarElement);
                        if (index == -1)
                            continue;

                        bar.PrimaryCommands[index] = e.NewItems[i] as ICommandBarElement;
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    bar.PrimaryCommands.Clear();

                    if (e.NewItems == null)
                        return;

                    foreach (var item in e.NewItems)
                        bar.PrimaryCommands.Add(item as ICommandBarElement);
                    break;
            }
        }

        private static readonly Dictionary<ObservableCollection<ICommandBarElement>, CommandBar> _bindings = 
            new Dictionary<ObservableCollection<ICommandBarElement>, CommandBar>();
    }
}
