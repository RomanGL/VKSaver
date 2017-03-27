using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Services.Transfer
{
    public static class TransferStatusExtensions
    {
        public static bool IsPaused(this VKSaverTransferStatus status)
        {
            switch (status)
            {
                case VKSaverTransferStatus.PausedByApplication:
                case VKSaverTransferStatus.Error:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsRunning(this VKSaverTransferStatus status)
        {
            return status == VKSaverTransferStatus.Running;
        }
    }
}
