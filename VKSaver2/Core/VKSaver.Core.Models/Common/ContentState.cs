namespace VKSaver.Core.Models.Common
{
    /// <summary>
    /// Перечисление состояний контента.
    /// </summary>
    public enum ContentState
    {
        /// <summary>
        /// Состояние отсутствует.
        /// </summary>
        None = 0,
        /// <summary>
        /// Данные отсутствуют.
        /// </summary>
        NoData,
        /// <summary>
        /// Контент загружен и может быть показан.
        /// </summary>
        Normal,
        /// <summary>
        /// Выполняется загрузка контента.
        /// </summary>
        Loading, 
        /// <summary>
        /// Произошла ошибка.
        /// </summary>
        Error
    }
}
