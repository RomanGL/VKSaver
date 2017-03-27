using System.Collections.Generic;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Services
{
    /// <summary>
    /// Используется для сравнения статуса загрузки.
    /// </summary>
    public class TransferStatusComparer : IComparer<VKSaverTransferStatus>
    {
        /// <summary>
        /// Сравнивает два элемента статуса передачи данных.
        /// </summary>
        /// <param name="x">Первый сравниваемый элемент.</param>
        /// <param name="y">Второй сравниваемый элемент.</param>
        public int Compare(VKSaverTransferStatus x, VKSaverTransferStatus y)
        {
            if (x == y) return 0;
            if (x == VKSaverTransferStatus.Running)
                return -1;
            else if (y == VKSaverTransferStatus.Running)
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
