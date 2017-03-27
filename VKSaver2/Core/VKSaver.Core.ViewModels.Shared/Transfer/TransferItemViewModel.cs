#if WINDOWS_UWP
using Prism.Windows.Mvvm;
#else
using Microsoft.Practices.Prism.StoreApps;
#endif

using System;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Services.Transfer;

namespace VKSaver.Core.ViewModels.Transfer
{
    public class TransferItemViewModel : ViewModelBase
    {
        public TransferItemViewModel(TransferItem item, ILocService locService)
        {
            Operation = item;
            _locService = locService;
        }

        public TransferItem Operation
        {
            get { return _operation; }
            set
            {
                _operation = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(TotalSize));
                OnPropertyChanged(nameof(ProcessedSize));
                OnPropertyChanged(nameof(Percentage));
                OnPropertyChanged(nameof(IsIndicatorPaused));
                OnPropertyChanged(nameof(Operation));
                OnPropertyChanged(nameof(SizeProgressText));
            }
        }
        
        public Guid OperationGuid { get { return _operation.OperationGuid; } }
        public string Name { get { return _operation.Name; } }
        public FileContentType ContentType { get { return _operation.ContentType; } }
        public VKSaverTransferStatus Status { get { return _operation.Status; } }
        public FileSize TotalSize { get { return _operation.TotalSize; } }
        public FileSize ProcessedSize { get { return _operation.ProcessedSize; } }

        public string SizeProgressText
        {
            get
            {
                return String.Format(_locService["TransferView_SizeMask_Text"],
                        ProcessedSize.ToFormattedString(_locService), TotalSize.ToFormattedString(_locService));
            }
        }

        public double Percentage
        {
            get
            {
                return TotalSize.Bytes == 0 ? 0: ProcessedSize.Bytes / (TotalSize.Bytes / 100);
            }
        }

        public bool IsIndicatorPaused
        {
            get
            {
                return Status == VKSaverTransferStatus.PausedByApplication ||
                    Status == VKSaverTransferStatus.PausedCostedNetwork ||
                    Status == VKSaverTransferStatus.PausedNoNetwork ||
                    Status == VKSaverTransferStatus.Error ||
                    Status == VKSaverTransferStatus.Idle;
            }
        }

        public bool CanPause { get { return Status.IsRunning(); } }

        public bool CanResume { get { return Status.IsPaused(); } }

        private TransferItem _operation;
        private readonly ILocService _locService;
    }
}
