namespace VKSaver.Core.Models.Transfer
{
    /// <summary>
    /// Тип ошибки при загрузке.
    /// </summary>
    public enum DownloadInitErrorType : byte
    {
        /// <summary>
        /// Неизвестная ошибка.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Не удалось создать папку для загрузки.
        /// </summary>
        CantCreateFolder,
        /// <summary>
        /// Не удалось создать файл для загрузки.
        /// </summary>
        CantCreateFile
    }
}
