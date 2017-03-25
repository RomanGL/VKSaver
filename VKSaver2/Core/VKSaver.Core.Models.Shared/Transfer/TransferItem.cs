using System;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Models.Transfer
{
    public class TransferItem
    {
        public VKSaverTransferStatus Status { get; set; }
        public Guid OperationGuid { get; set; }
        public string Name { get; set; }
        public FileContentType ContentType { get; set; }
        public FileSize TotalSize { get; set; }
        public FileSize ProcessedSize { get; set; }
    }
}
