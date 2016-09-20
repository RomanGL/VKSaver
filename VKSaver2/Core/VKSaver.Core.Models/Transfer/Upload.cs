namespace VKSaver.Core.Models.Transfer
{
    public sealed class Upload : IUpload
    {
        public IUploadable Uploadable { get; set; }

        public string UploadUrl { get; set; }
    }
}
