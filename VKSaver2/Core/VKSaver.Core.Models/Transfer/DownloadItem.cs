using System;
using VKSaver.Core.Models.Common;
using Windows.Networking.BackgroundTransfer;

namespace VKSaver.Core.Transfer
{
    /// <summary>
    /// Представляет элемент списка загрузок.
    /// </summary>
    public class DownloadItem
    {
        /// <summary>
        /// Статус операции загрузки.
        /// </summary>
        public BackgroundTransferStatus Status { get; set; }
        /// <summary>
        /// Уникальный идентификатор загрузки.
        /// </summary>
        public Guid OperationGuid { get; set; }
        /// <summary>
        /// Название загрузки.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Тип содержимого загрузки.
        /// </summary>
        public FileContentType ContentType { get; set; }
        /// <summary>
        /// Общий размер загружаемых данных.
        /// </summary>
        public FileSize TotalSize { get; set; }
        /// <summary>
        /// Размер загруженных данных.
        /// </summary>
        public FileSize DownloadedSize { get; set; }
    }
}
