using System;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.ViewModels.Collections
{
    /// <summary>
    /// Представляет информацию о событии StateChanged.
    /// </summary>
    public sealed class StateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Возвращает новое состояние данных.
        /// </summary>
        public ContentState State { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="StateChangedEventArgs"/>.
        /// </summary>
        /// <param name="state">Новое состояние данных.</param>
        internal StateChangedEventArgs(ContentState state)
        {
            State = state;
        }
    }
}
