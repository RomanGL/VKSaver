namespace VKSaver.Core.Models.Common
{
    public static class FileContentTypeExtensions
    {
        public static FileContentType GetContentTypeFromExtension(string fileExtension)
        {
            switch (fileExtension)
            {
                case ".mp3":
                case ".wma":
                case ".wav":
                case ".aac":
                case ".m4a":
                case ".flac":
                    return FileContentType.Music;

                case ".mp4":
                case ".mkv":
                case ".avi":
                case ".3gp":
                    return FileContentType.Video;

                case ".jpg":
                case ".jpe":
                case ".jpeg":
                case ".bmp":
                case ".png":
                case ".gif":
                    return FileContentType.Image;

                default: return FileContentType.Other;
            }
        }
    }
}
