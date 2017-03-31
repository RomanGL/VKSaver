#if WINDOWS_UWP
using Prism.Commands;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
#elif ANDROID
using VKSaver.Core.Toolkit.Commands;
#endif

using VKSaver.Core.Models.Common;

namespace VKSaver.Core.ViewModels.Collections
{
    /// <summary>
    /// Представляет метод, обрабатывающий данные <see cref="StateChangedEventArgs"/> события StateChanged.
    /// </summary>
    /// <param name="sender">Инициатор события.</param>
    /// <param name="e">Информация о событии.</param>
    public delegate void StateChangedEventHandler(object sender, StateChangedEventArgs e);

    /// <summary>
    /// Представляет коллекцию с поддержкой состояние данных.
    /// </summary>
    public interface IStateProviderCollection
    {
        /// <summary>
        /// Возвращает команду для загрузки элементов.
        /// </summary>
        DelegateCommand LoadCommand { get; }
        /// <summary>
        /// Возвращает состояние данных.
        /// </summary>
        ContentState ContentState { get; }
        /// <summary>
        /// Происходит при изменении состояния данных.
        /// </summary>
        event StateChangedEventHandler StateChanged;
        /// <summary>
        /// Загружает элементы в коллекцию.
        /// </summary>
        void Load();
        /// <summary>
        /// Обновляет коллекцию.
        /// </summary>
        void Refresh();
    }
}
