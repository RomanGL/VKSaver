using Microsoft.Xaml.Interactivity;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VKSaver.Behaviors
{
    public sealed class CommandBarButtonsBindingBehavior : DependencyObject, IBehavior
    {
        public DependencyObject AssociatedObject { get; private set; }

        private CommandBar AttachedBar { get; set; }

        public ObservableCollection<ICommandBarElement> PrimaryCommands
        {
            get { return (ObservableCollection<ICommandBarElement>)GetValue(PrimaryCommandsProperty); }
            set { SetValue(PrimaryCommandsProperty, value); }
        }
        
        public static readonly DependencyProperty PrimaryCommandsProperty =
            DependencyProperty.Register("PrimaryCommands", typeof(ObservableCollection<ICommandBarElement>), 
                typeof(CommandBarButtonsBindingBehavior), new PropertyMetadata(null, OnPrimaryCommandsChanged));

        public ObservableCollection<ICommandBarElement> SecondaryCommands
        {
            get { return (ObservableCollection<ICommandBarElement>)GetValue(SecondaryCommandsProperty); }
            set { SetValue(SecondaryCommandsProperty, value); }
        }

        public static readonly DependencyProperty SecondaryCommandsProperty =
            DependencyProperty.Register("SecondaryCommands", typeof(ObservableCollection<ICommandBarElement>),
                typeof(CommandBarButtonsBindingBehavior), new PropertyMetadata(null, OnSecondaryCommandsChanged));

        public void Attach(DependencyObject associatedObject)
        {
            AssociatedObject = associatedObject;
            AttachedBar = associatedObject as CommandBar;
        }

        public void Detach()
        {
            if (_isPrimaryCollectionAttached)
                PrimaryCommands.CollectionChanged -= PrimaryCommands_Changed;
            if (_isSecondaryCollectionAttached)
                SecondaryCommands.CollectionChanged -= SecondaryCommands_Changed;

            AssociatedObject = null;
            AttachedBar = null;
        }

        private void PrimaryCommands_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateButtons(AttachedBar.PrimaryCommands, e);

            if (AttachedBar.Visibility == Visibility.Collapsed &&
                AttachedBar.Tag == null)
                AttachedBar.Visibility = Visibility.Visible;
        }

        private void SecondaryCommands_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateButtons(AttachedBar.SecondaryCommands, e);

            if (AttachedBar.Visibility == Visibility.Collapsed &&
                AttachedBar.Tag == null)
                AttachedBar.Visibility = Visibility.Visible;
        }

        private static void UpdateButtons(IList<ICommandBarElement> barElements, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                        barElements.Add(item as ICommandBarElement);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                        barElements.Remove(item as ICommandBarElement);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        int index = barElements.IndexOf(e.OldItems[i] as ICommandBarElement);
                        if (index == -1)
                            continue;

                        barElements[index] = e.NewItems[i] as ICommandBarElement;
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    barElements.Clear();
                    if (e.NewItems == null)
                        return;

                    foreach (var item in e.NewItems)
                        barElements.Add(item as ICommandBarElement);
                    break;
            }
        }

        private static void OnPrimaryCommandsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var behavior = obj as CommandBarButtonsBindingBehavior;
            if (behavior.AttachedBar != null)
                behavior.AttachedBar.PrimaryCommands.Clear();

            if (e.OldValue != null && behavior._isPrimaryCollectionAttached)
            {
                var collection = e.OldValue as ObservableCollection<ICommandBarElement>;
                collection.CollectionChanged -= behavior.PrimaryCommands_Changed;
                behavior._isPrimaryCollectionAttached = false;
            }
            if (e.NewValue != null && !behavior._isPrimaryCollectionAttached)
            {
                var collection = e.NewValue as ObservableCollection<ICommandBarElement>;
                collection.CollectionChanged += behavior.PrimaryCommands_Changed;
                behavior._isPrimaryCollectionAttached = true;

                if (behavior.AttachedBar != null)
                {
                    foreach (var item in collection)
                        behavior.AttachedBar.PrimaryCommands.Add(item);

                    if (behavior.AttachedBar.Visibility == Visibility.Collapsed && 
                        behavior.AttachedBar.Tag == null && collection.Any())
                        behavior.AttachedBar.Visibility = Visibility.Visible;
                }
            }
        }

        private static void OnSecondaryCommandsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var behavior = obj as CommandBarButtonsBindingBehavior;
            if (behavior.AttachedBar != null)
                behavior.AttachedBar.SecondaryCommands.Clear();

            if (e.OldValue != null && behavior._isSecondaryCollectionAttached)
            {
                var collection = e.OldValue as ObservableCollection<ICommandBarElement>;
                collection.CollectionChanged -= behavior.SecondaryCommands_Changed;
                behavior._isSecondaryCollectionAttached = false;
            }
            if (e.NewValue != null && !behavior._isSecondaryCollectionAttached)
            {
                var collection = e.NewValue as ObservableCollection<ICommandBarElement>;
                collection.CollectionChanged += behavior.SecondaryCommands_Changed;
                behavior._isSecondaryCollectionAttached = true;

                if (behavior.AttachedBar != null)
                {
                    foreach (var item in collection)
                        behavior.AttachedBar.SecondaryCommands.Add(item);

                    if (behavior.AttachedBar.Visibility == Visibility.Collapsed &&
                        behavior.AttachedBar.Tag == null && collection.Any())
                        behavior.AttachedBar.Visibility = Visibility.Visible;
                }
            }
        }

        private bool _isPrimaryCollectionAttached;
        private bool _isSecondaryCollectionAttached;
    }
}
