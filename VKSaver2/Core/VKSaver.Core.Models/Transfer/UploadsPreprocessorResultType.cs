namespace VKSaver.Core.Models.Transfer
{
    public enum UploadsPreprocessorResultType : byte
    {
        Success = 0,
        ConnectionError,
        ServerError,
        UnknownError
    }
}
