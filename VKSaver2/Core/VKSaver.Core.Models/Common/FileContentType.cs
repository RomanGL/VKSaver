namespace VKSaver.Core.Models.Common
{
    /// <summary>
    /// Тип содержимого файла.
    /// </summary>
    public enum FileContentType
    {
        /// <summary>
        /// Любой тип содержимого. Загружается в папку "Документы".
        /// </summary>
        Other = 0,
        /// <summary>
        /// Аудиофайл. Загружается в папку "Музыка".
        /// </summary>
        Music,
        /// <summary>
        /// Видеофайл. Загружается в папку "Видео".
        /// </summary>
        Video,
        /// <summary>
        /// Изображение. Загружается в папку "Изображения".
        /// </summary>
        Image
    }
}
