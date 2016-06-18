using System;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Services
{
    /// <summary>
    /// Представляет данные о событии ошибки в операции загрузки.
    /// </summary>
    public class DownloadOperationErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Уникальный идентификатор операции загрузки.
        /// </summary>
        public Guid OperationGuid { get; private set; }
        /// <summary>
        /// Название операции загрузки.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Тип содержимого операции загрузки.
        /// </summary>
        public FileContentType ContentType { get; private set; }
        /// <summary>
        /// Исключение, которое произошло при загрузке.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса с заданной информации об ошибке операции загрузки.
        /// </summary>
        /// <param name="guid">Уникальный идентификатор загрузки.</param>
        /// <param name="name">Название загрузки.</param>
        /// <param name="type">Тип содержимого загрузки.</param>
        /// <param name="exception">Произошедшее исключение.</param>
        public DownloadOperationErrorEventArgs(Guid guid, string name, FileContentType type, Exception exception)
        {
            OperationGuid = guid;
            Name = name;
            ContentType = type;
            Exception = exception;
        }
    }
}
