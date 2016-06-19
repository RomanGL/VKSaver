using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace VKSaver.Core.ViewModels.Collections
{
    public abstract class StateSupportMultiSelectCollection<T> : StateSupportCollection<T>,
        IMultiSelectCollectionView
    {
        protected StateSupportMultiSelectCollection(IEnumerable<T> collection)
            : base(collection)
        {
            SelectedItems = new List<T>();
        }
        
        protected StateSupportMultiSelectCollection()
        {
            SelectedItems = new List<T>();
        }

        public List<T> SelectedItems { get; private set; }

        public void AddControl(Selector selector)
        {
            _controls.Add(selector);
            SetSelection(selector);
            selector.SelectionChanged += control_SelectionChanged;
        }

        public void RemoveControl(Selector selector)
        {
            if (_controls.Remove(selector))
                selector.SelectionChanged -= control_SelectionChanged;
        }

        private void SetSelection(Selector selector)
        {
            var listView = selector as ListViewBase;
            var listBox = selector as ListBox;

            if (listView != null)
            {
                listView.SelectedItems.Clear();

                foreach (T item in SelectedItems)
                    listView.SelectedItems.Add(item);
            }
            else if (listBox != null)
            {
                listBox.SelectedItems.Clear();

                foreach (T item in SelectedItems)
                    listBox.SelectedItems.Add(item);
            }
        }

        private void control_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_ignoreSelectionChanged)
            {
                bool changed = false;
                _ignoreSelectionChanged = true;

                try
                {
                    foreach (T item in e.AddedItems)
                    {
                        if (!SelectedItems.Contains(item))
                        {
                            SelectedItems.Add(item);
                            changed = true;
                        }
                    }

                    foreach (T item in e.RemovedItems)
                    {
                        if (SelectedItems.Remove(item))
                            changed = true;
                    }

                    if (changed)
                    {
                        foreach (Selector control in _controls)
                        {
                            if (control != sender)
                                SetSelection(control);
                        }
                    }
                }
                finally
                {
                    _ignoreSelectionChanged = false;
                }
            }
        }

        private bool _ignoreSelectionChanged;        
        private readonly List<Selector> _controls = new List<Selector>();
    }
}
