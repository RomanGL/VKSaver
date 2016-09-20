namespace VKSaver.Core.Models.Transfer
{
    public sealed class UploadInitError
    {
        public UploadInitError(UploadInitErrorType errorType, IUploadable upload)
        {
            ErrorType = errorType;
            Upload = upload;
        }

        public UploadInitErrorType ErrorType { get; private set; }

        public IUploadable Upload { get; private set; }
    }
}
