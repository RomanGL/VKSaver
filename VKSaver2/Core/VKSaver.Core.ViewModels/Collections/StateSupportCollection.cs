using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Microsoft.Practices.Prism.StoreApps;
using VKSaver.Core.Models.Common;
using Newtonsoft.Json;

namespace VKSaver.Core.ViewModels.Collections
{
    /// <summary>
    /// Represents a base observable collection that provides notification when content state changed.
    /// </summary>
    /// <typeparam name="T">The type of elements in collection.</typeparam>
    public abstract class StateSupportCollection<T> : ObservableCollection<T>, IStateProviderCollection
    {
        private ContentState _state = ContentState.Normal;

        /// <summary>
        /// Gets a command of load data to collection.
        /// </summary>
        [JsonIgnore]
        public DelegateCommand LoadCommand { get; private set; }

        /// <summary>
        /// Возвращает команду обновления коллекции.
        /// </summary>
        [JsonIgnore]
        public DelegateCommand RefreshCommand { get; private set; }

        /// <summary>
        /// Gets current content state.
        /// </summary>
        public ContentState ContentState
        {
            get { return this._state; }
            set
            {
                if (value == this._state)
                    return;

                this._state = value;

                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ContentState)));
                this.OnStateChanged();
            }
        }

        /// <summary>
        /// Occurs when a state value changed.
        /// </summary>
        public event StateChangedEventHandler StateChanged;

        /// <summary>
        /// Load data to collection.
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Refresh a collection. You must prepare collection to refresh and call base method.
        /// </summary>
        public virtual void Refresh()
        {
            this.Clear();
            this.Reset();
            this.Load();
        }

        /// <summary>
        /// Reset collection to default values.
        /// </summary>
        protected virtual void Reset()
        {

        }

        /// <summary>
        /// Invoked when a state changed.
        /// </summary>
        private async void OnStateChanged()
        {
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, (() =>
            {
                this.OnPropertyChanged(new PropertyChangedEventArgs("ContentState"));
                if (this.StateChanged != null)
                    this.StateChanged(this, new StateChangedEventArgs(ContentState));
            }));            
        }

        /// <summary>
        /// Creates the new instance of <see cref="StateSupportCollection"/> class.
        /// </summary>
        /// <param name="collection">Collection with elements of type <paramref name="T"/>.</param>
        protected StateSupportCollection(IEnumerable<T> collection)
            : base(collection)
        {
            this.LoadCommand = new DelegateCommand(() => this.Load());
            this.RefreshCommand = new DelegateCommand(() => this.Refresh());
        }

        /// <summary>
        /// Initializes a new instance of <see cref="StateSupportCollection{T}"/> class.
        /// </summary>
        protected StateSupportCollection()
        {
            this.LoadCommand = new DelegateCommand(() => this.Load());
            this.RefreshCommand = new DelegateCommand(() => this.Refresh());
        }
    }
}
