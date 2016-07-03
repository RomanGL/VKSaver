using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.ViewModels.Collections
{
    /// <summary>
    /// Представляет простую реализацию <see cref="StateSupportCollection{T}"/>.
    /// Не поддерживает инкрементную загрузку.
    /// </summary>
    public class SimpleStateSupportCollection<T> : StateSupportCollection<T>
    {
        public SimpleStateSupportCollection()
            : base() { }

        public SimpleStateSupportCollection(IEnumerable<T> collection)
            : base(collection) { }

        public SimpleStateSupportCollection(Func<Task<IEnumerable<T>>> loadItemsFunc)
            :base()
        {
            LoadItems = loadItemsFunc;
        }

        public SimpleStateSupportCollection(IEnumerable<T> collection, Func<Task<IEnumerable<T>>> loadItemsFunc)
            : base(collection)
        {
            LoadItems = loadItemsFunc;
        }
                
        public Func<Task<IEnumerable<T>>> LoadItems { get; set; }
        
        public override async void Load()
        {
            try
            {
                ContentState = ContentState.Loading;

                List<T> data = new List<T>();
                data = (await LoadItems()).ToList();

                await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    foreach (var item in data)
                        this.Add(item);
                });

                ContentState = this.Any() ? ContentState.Normal : ContentState.NoData;
            }
            catch (Exception ex)
            {
                ContentState = ContentState.Error;
            }
        }
    }
}
