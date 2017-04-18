using Microsoft.Xaml.Interactivity;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VKSaver.Common;
using VKSaver.Core.ViewModels.Collections;
using Windows.UI.Xaml.Data;

namespace VKSaver.Behaviors
{
    /// <summary>
    /// Представляет поведение для списочных элементов управления. 
    /// Обеспечивает активацию логики подгрузки элементов в начало и в конец списка.
    /// </summary>
    public class IncrementalUpDownLoadingBehavior : DependencyObject, IBehavior
    {
        private ScrollViewer sv;
        private ListViewBase listView;
        private DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        private bool isUpWorks;
        private bool isDownWorks;

        /// <summary>
        /// Объект зависимостей, к которому присоединяется поведение.
        /// </summary>
        public DependencyObject AssociatedObject { get; private set; }

        #region Dependency Properties

        /// <summary>
        /// Количество элементов для подгрузки.
        /// </summary>
        public int NumberItemsToLoad
        {
            get { return (int)GetValue(NumberItemsToLoadProperty); }
            set { SetValue(NumberItemsToLoadProperty, value); }
        }
        
        public static readonly DependencyProperty NumberItemsToLoadProperty =
            DependencyProperty.Register("NumberItemsToLoad", typeof(int), 
                typeof(IncrementalUpDownLoadingBehavior), new PropertyMetadata(20));
        
        /// <summary>
        /// Высота от края списка для срабатывания триггера прокрутки.
        /// </summary>
        public int ScrollHeightTriggerOffset
        {
            get { return (int)GetValue(ScrollHeightTriggerOffsetProperty); }
            set { SetValue(ScrollHeightTriggerOffsetProperty, value); }
        }
        
        public static readonly DependencyProperty ScrollHeightTriggerOffsetProperty =
            DependencyProperty.Register("ScrollHeightTriggerOffset", typeof(int), 
                typeof(IncrementalUpDownLoadingBehavior), new PropertyMetadata(500));

        /// <summary>
        /// Коллекция, к которой привязан список. Может быть null.
        /// </summary>
        public object ItemsSource
        {
            get { return (object)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(object), 
                typeof(IncrementalUpDownLoadingBehavior), new PropertyMetadata(null));

        #endregion

        /// <summary>
        /// Присоединяет поведние к объекту зависимостей.
        /// </summary>
        /// <param name="associatedObject">Объект, к которому присоединяется поведение.</param>
        public void Attach(DependencyObject associatedObject)
        {
            AssociatedObject = associatedObject;

            listView = associatedObject as ListViewBase;
            if (listView == null) return;

            timer.Tick += Timer_Tick;
            listView.Loaded += (s, e) =>
            {
                sv = listView.GetFirstOrDefaultDescendantOfType<ScrollViewer>();
                if (sv == null) return;

                Timer_Tick(null, null);
                timer.Start();
            };
        }

        /// <summary>
        /// Взывается при срабатывании таймера.
        /// </summary>
        private void Timer_Tick(object sender, object e)
        {
            if (NumberItemsToLoad <= 0) return;

            int verticalOffset = (int)sv.VerticalOffset;
            if (!isDownWorks && verticalOffset + listView.ActualHeight + ScrollHeightTriggerOffset >= sv.ExtentHeight)
                ProcessDown();
            if (!isUpWorks && verticalOffset - ScrollHeightTriggerOffset <= 0)
                ProcessUp();
        }

        /// <summary>
        /// Обработать элементы вверху.
        /// </summary>
        private async void ProcessUp()
        {
            isUpWorks = true;
            ISupportUpDownIncrementalLoading collection = null;

            if (ItemsSource != null)
            {
                collection = ItemsSource as ISupportUpDownIncrementalLoading;

                if (collection == null)
                {
                    var source = ItemsSource as CollectionViewSource;
                    if (source != null)
                        collection = source.Source as ISupportUpDownIncrementalLoading;
                }
            }
            else
                collection = listView.ItemsSource as ISupportUpDownIncrementalLoading;

            if (collection != null && collection.HasMoreUpItems)
            {
                var itemToScroll = await collection.LoadUpAsync((uint)NumberItemsToLoad);
                if (itemToScroll != null) listView.ScrollIntoView(itemToScroll);
                if (sv.VerticalOffset == 0) sv.ChangeView(null, 1, null, true);
            }

            isUpWorks = false;
        }

        /// <summary>
        /// Обработать элементы внизу.
        /// </summary>
        private async void ProcessDown()
        {
            isDownWorks = true;

            ISupportUpDownIncrementalLoading collection = null;

            if (ItemsSource != null)
            {
                collection = ItemsSource as ISupportUpDownIncrementalLoading;

                if (collection == null)
                {
                    var source = ItemsSource as CollectionViewSource;
                    if (source != null)
                        collection = source.Source as ISupportUpDownIncrementalLoading;
                }
            }
            else
                collection = listView.ItemsSource as ISupportUpDownIncrementalLoading;

            if (collection != null && collection.HasMoreDownItems)
            {
                var itemToScroll = await collection.LoadDownAsync((uint)NumberItemsToLoad);
                if (itemToScroll != null) listView.ScrollIntoView(itemToScroll);
            }

            isDownWorks = false;
        }

        /// <summary>
        /// Отсоединяет поведение от объекта зависимостей.
        /// </summary>
        public void Detach()
        {
            timer.Stop();
            timer.Tick -= Timer_Tick;

            isUpWorks = false;
            isDownWorks = false;

            sv = null;
            listView = null;
            AssociatedObject = null;
        }
    }
}
