namespace VKSaver.Core.Models.Common
{
    public enum VKSaverTransferStatus
    {
        Idle = 0,
        Running = 1,
        PausedByApplication = 2,
        PausedCostedNetwork = 3,
        PausedNoNetwork = 4,
        Completed = 5,
        Canceled = 6,
        Error = 7,
        PausedSystemPolicy = 32
    }
}
