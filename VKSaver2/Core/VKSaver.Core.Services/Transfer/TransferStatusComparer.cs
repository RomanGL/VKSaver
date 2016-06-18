using System.Collections.Generic;
using Windows.Networking.BackgroundTransfer;

namespace VKSaver.Core.Services
{
    /// <summary>
    /// Используется для сравнения статуса загрузки.
    /// </summary>
    public class TransferStatusComparer : IComparer<BackgroundTransferStatus>
    {
        /// <summary>
        /// Сравнивает два элемента статуса передачи данных.
        /// </summary>
        /// <param name="x">Первый сравниваемый элемент.</param>
        /// <param name="y">Второй сравниваемый элемент.</param>
        public int Compare(BackgroundTransferStatus x, BackgroundTransferStatus y)
        {
            if (x == y) return 0;
            return x == BackgroundTransferStatus.Running ? 1 : -1;
        }
    }
}
