namespace VKSaver.Core.Models.Transfer
{
    public sealed class UploadInitError
    {
        public UploadInitError(UploadInitErrorType errorType, IUpload upload)
        {
            ErrorType = errorType;
            Upload = upload;
        }

        public UploadInitErrorType ErrorType { get; private set; }

        public IUpload Upload { get; private set; }
    }
}
