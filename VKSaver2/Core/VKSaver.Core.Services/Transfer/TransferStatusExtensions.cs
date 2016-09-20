using Windows.Networking.BackgroundTransfer;

namespace VKSaver.Core.Services.Transfer
{
    public static class TransferStatusExtensions
    {
        public static bool IsPaused(this BackgroundTransferStatus status)
        {
            switch (status)
            {
                case BackgroundTransferStatus.PausedByApplication:
                case BackgroundTransferStatus.Error:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsRunning(this BackgroundTransferStatus status)
        {
            return status == BackgroundTransferStatus.Running;
        }
    }
}
