using System;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Services
{
    public class TransferOperationErrorEventArgs : EventArgs
    {
        public Guid OperationGuid { get; private set; }
        public string Name { get; private set; }
        public FileContentType ContentType { get; private set; }
        public Exception Exception { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TransferOperationErrorEventArgs"/> 
        /// с заданной информации об ошибке операции передачи данных.
        /// </summary>
        /// <param name="guid">Уникальный идентификатор операции.</param>
        /// <param name="name">Название операции.</param>
        /// <param name="type">Тип содержимого.</param>
        /// <param name="exception">Произошедшее исключение.</param>
        public TransferOperationErrorEventArgs(Guid guid, string name, FileContentType type, Exception exception)
        {
            OperationGuid = guid;
            Name = name;
            ContentType = type;
            Exception = exception;
        }
    }
}
