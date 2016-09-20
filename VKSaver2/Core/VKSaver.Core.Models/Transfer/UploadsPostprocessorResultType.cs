namespace VKSaver.Core.Models.Transfer
{
    public enum UploadsPostprocessorResultType : byte
    {
        Unknown = 0,
        Success,
        ConnectionError,
        ServerError
    }
}
