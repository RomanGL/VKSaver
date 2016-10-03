namespace VKSaver.Core.Models.Transfer
{
    public interface IUpload
    {
        string UploadUrl { get; }

        IUploadable Uploadable { get; }
    }
}
