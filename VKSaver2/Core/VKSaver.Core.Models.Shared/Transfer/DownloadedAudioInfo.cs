namespace VKSaver.Core.Models.Transfer
{
    /// <summary>
    /// Представляет информацию о загруженной аудиозаписи.
    /// </summary>
    public class DownloadedAudioInfo
    {
        /// <summary>
        /// Идентификатор аудиозаписи.
        /// </summary>
        public ulong ID { get; set; }
        /// <summary>
        /// Идентификатор владельца аудиозаписи.
        /// </summary>
        public long OwnerID { get; set; }
        /// <summary>
        /// Заголовк аудиозаписи.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Исполнитель аудиозаписи.
        /// </summary>
        public string Artist { get; set; }
    }
}
