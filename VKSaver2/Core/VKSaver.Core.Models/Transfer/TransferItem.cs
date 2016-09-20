using System;
using VKSaver.Core.Models.Common;
using Windows.Networking.BackgroundTransfer;

namespace VKSaver.Core.Models.Transfer
{
    public class TransferItem
    {
        public BackgroundTransferStatus Status { get; set; }
        public Guid OperationGuid { get; set; }
        public string Name { get; set; }
        public FileContentType ContentType { get; set; }
        public FileSize TotalSize { get; set; }
        public FileSize ProcessedSize { get; set; }
    }
}
