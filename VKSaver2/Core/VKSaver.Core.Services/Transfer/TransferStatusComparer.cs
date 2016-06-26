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
            if (x == BackgroundTransferStatus.Running)
                return -1;
            else if (y == BackgroundTransferStatus.Running)
                return 1;
            else
            {
                int xn = (int)x;
                int yn = (int)y;

                return xn > yn ? -1 : 1;
            }
        }
    }
}
