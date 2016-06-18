namespace VKSaver.Core.Models.Transfer
{
    /// <summary>
    /// Представляет информацию об ошибке при загрузке.
    /// </summary>
    public class DownloadInitError
    {
        /// <summary>
        /// Тип произошедшей ошибки.
        /// </summary>
        public DownloadInitErrorType ErrorType { get; private set; }
        /// <summary>
        /// Элемент, который не удалось загрузить.
        /// </summary>
        public IDownloadable DownloadItem { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса с информацией об ошибке старта загрузки.
        /// </summary>
        /// <param name="type">Тип ошибки.</param>
        /// <param name="item">Элемент, который не удалось загрузить.</param>
        public DownloadInitError(DownloadInitErrorType type, IDownloadable item)
        {
            ErrorType = type;
            DownloadItem = item;
        }
    }
}
