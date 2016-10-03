namespace VKSaver.Core.Services.Interfaces
{
    /// <summary>
    /// Представляет сервис настроек приложения.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Очистить хранилище настроек приложения.
        /// </summary>
        void Clear();

        /// <summary>
        /// Записать параметр в хранилище настроек.
        /// </summary>
        /// <typeparam name="T">Тип параметра.</typeparam>
        /// <param name="parameterName">Имя параметра.</param>
        /// <param name="value">Значение параметра.</param>
        void Set<T>(string parameterName, T value);

        /// <summary>
        /// Получить параметр из хранилища настроек.
        /// </summary>
        /// <typeparam name="T">Тип параметра.</typeparam>
        /// <param name="parameterName">Имя параметра.</param>
        /// <param name="defaultValue">Значение по умолчанию для параметра, 
        /// если он отсутствует в хранилище.</param>
        T Get<T>(string parameterName, T defaultValue = default(T));

        /// <summary>
        /// Получить параметр из хранилища настроек без использоваия кэшированного значения.
        /// </summary>
        /// <typeparam name="T">Тип параметра.</typeparam>
        /// <param name="parameterName">Имя параметра.</param>
        /// <param name="defaultValue">Значение по умолчанию для параметра,
        /// если он отсутствует в хранилище.</param>
        T GetNoCache<T>(string parameterName, T defaultValue = default(T));

        /// <summary>
        /// Удалить параметр из хранилища.
        /// </summary>
        /// <param name="parameterName">Имя параметра.</param>
        void Remove(string parameterName);

        /// <summary>
        /// Возвращает значение, содержится ли в хранилище настроек параметр
        /// с указанным именем.
        /// </summary>
        /// <param name="parameterName">Имя параметра.</param>
        bool ContainsSetting(string parameterName);
    }
}
