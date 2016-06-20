using Microsoft.Xaml.Interactivity;
using System.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VKSaver.Behaviors
{
    public sealed class SelectedItemsBindingBehavior : WithCommandDependencyObject, IBehavior
    {
        public DependencyObject AssociatedObject { get; private set; }

        private ListViewBase AttachedListView { get; set; }

        private ListBox AttachedListBox { get; set; }

        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }
        
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IList), 
                typeof(SelectedItemsBindingBehavior), new PropertyMetadata(null, OnSelectedItemsChanged));

        public void Attach(DependencyObject associatedObject)
        {
            AssociatedObject = associatedObject;
            AttachedListView = associatedObject as ListViewBase;
            AttachedListBox = associatedObject as ListBox;

            if (AttachedListView != null)
                AttachedListView.SelectionChanged += SelectionChanged;
            else if (AttachedListBox != null)
                AttachedListBox.SelectionChanged += SelectionChanged;
        }

        public void Detach()
        {
            if (AttachedListView != null)
                AttachedListView.SelectionChanged -= SelectionChanged;
            else if (AttachedListBox != null)
                AttachedListBox.SelectionChanged -= SelectionChanged;

            AssociatedObject = null;
            AttachedListView = null;
            AttachedListBox = null;
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedItems == null)
                return;

            foreach (var item in e.RemovedItems)
                SelectedItems.Remove(item);

            foreach (var item in e.AddedItems)
                SelectedItems.Add(item);

            if (Command != null && Command.CanExecute(CommandParameter))
                Command.Execute(CommandParameter);
        }

        private static void OnSelectedItemsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (SelectedItemsBindingBehavior)obj;
            var list = e.NewValue as IList;
            if (list == null)
                return;

            if (behavior.AttachedListView != null)
            {
                foreach (var item in behavior.AttachedListView.SelectedItems)
                    list.Add(item);
            }
            else if (behavior.AttachedListBox != null)
            {
                foreach (var item in behavior.AttachedListBox.SelectedItems)
                    list.Add(item);
            }
        }
    }
}
