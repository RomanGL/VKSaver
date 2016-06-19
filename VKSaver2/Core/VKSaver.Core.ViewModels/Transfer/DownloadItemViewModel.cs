using Microsoft.Practices.Prism.StoreApps;
using System;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Transfer;
using VKSaver.Core.Transfer;
using Windows.Networking.BackgroundTransfer;

namespace VKSaver.Core.ViewModels.Transfer
{
    /// <summary>
    /// Представляет модель представления загрузки.
    /// </summary>
    public class DownloadItemViewModel : ViewModelBase
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса с заданной информацией о загрузке.
        /// </summary>
        /// <param name="item">Объект с информацией о загрузке.</param>
        public DownloadItemViewModel(DownloadItem item)
        {
            Download = item;
        }

        private DownloadItem _download;

        /// <summary>
        /// Объект с информацией о загрузке.
        /// </summary>
        public DownloadItem Download
        {
            get { return _download; }
            set
            {
                _download = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(TotalSize));
                OnPropertyChanged(nameof(DownloadedSize));
                OnPropertyChanged(nameof(Percentage));
                OnPropertyChanged(nameof(IsIndicatorPaused));
                OnPropertyChanged(nameof(Download));
            }
        }

        /// <summary>
        /// Уникальный идентификатор загрузки.
        /// </summary>
        public Guid OpeartionGuid { get { return _download.OperationGuid; } }
        /// <summary>
        /// Название загрузки.
        /// </summary>
        public string Name { get { return _download.Name; } }
        /// <summary>
        /// Тип содержимого загрузки.
        /// </summary>
        public FileContentType ContentType { get { return _download.ContentType; } }
        /// <summary>
        /// Статус загрузки.
        /// </summary>
        public BackgroundTransferStatus Status { get { return _download.Status; } }
        /// <summary>
        /// Общий размер для загрузки.
        /// </summary>
        public FileSize TotalSize { get { return _download.TotalSize; } }
        /// <summary>
        /// Размер загруженных данных.
        /// </summary>
        public FileSize DownloadedSize { get { return _download.DownloadedSize; } }
        /// <summary>
        /// Процент выполнения операции загрузки.
        /// </summary>
        public double Percentage
        {
            get
            {
                return TotalSize.Bytes == 0 ? 0: DownloadedSize.Bytes / (TotalSize.Bytes / 100);
            }
        }
        /// <summary>
        /// Указывает на необходимость приостановки индикатора.
        /// </summary>
        public bool IsIndicatorPaused
        {
            get
            {
                return Status == BackgroundTransferStatus.PausedByApplication ||
                    Status == BackgroundTransferStatus.PausedCostedNetwork ||
                    Status == BackgroundTransferStatus.PausedNoNetwork ||
                    Status == BackgroundTransferStatus.Error ||
                    Status == BackgroundTransferStatus.Idle;
            }
        }

        public bool CanPause { get { return Status.IsRunning(); } }

        public bool CanResume { get { return Status.IsPaused(); } }
    }
}
