using Windows.Storage;
using Newtonsoft.Json;
using System.Collections.Generic;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class SettingsService : ISettingsService
    {
        private readonly Dictionary<string, object> cache = new Dictionary<string, object>();
        private readonly object lockObject = new object();

        /// <summary>
        /// Очистить хранилище настроек приложения.
        /// </summary>
        public void Clear()
        {
            lock (lockObject)
            {
                ApplicationData.Current.LocalSettings.Values.Clear();
                cache.Clear();
            }
        }

        /// <summary>
        /// Записать параметр в хранилище настроек.
        /// </summary>
        /// <typeparam name="T">Тип параметра.</typeparam>
        /// <param name="parameterName">Имя параметра.</param>
        /// <param name="value">Значение параметра.</param>
        public void Set<T>(string parameterName, T value)
        {
            string json = JsonConvert.SerializeObject(value);

            lock (lockObject)
            {
                ApplicationData.Current.LocalSettings.Values[parameterName] = json;
                cache[parameterName] = value;
            }
        }

        /// <summary>
        /// Получить параметр из хранилища настроек.
        /// </summary>
        /// <typeparam name="T">Тип параметра.</typeparam>
        /// <param name="parameterName">Имя параметра.</param>
        /// <param name="defaultValue">Значение по умолчанию для параметра, 
        /// если он отсутствует в хранилище.</param>
        public T Get<T>(string parameterName, T defaultValue = default(T))
        {
            lock (lockObject)
            {
                object readed = null;
                cache.TryGetValue(parameterName, out readed);

                if (readed != null)
                    return (T)readed;

                ApplicationData.Current.LocalSettings.Values.TryGetValue(parameterName, out readed);
                if (readed != null)
                {
                    string json = readed.ToString();
                    T value = JsonConvert.DeserializeObject<T>(json);

                    cache[parameterName] = value;
                    return value;
                }
                else
                {
                    string json = JsonConvert.SerializeObject(defaultValue);
                    ApplicationData.Current.LocalSettings.Values[parameterName] = json;
                    cache[parameterName] = defaultValue;
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Получить параметр из хранилища настроек без использования кэшированного значения..
        /// </summary>
        /// <typeparam name="T">Тип параметра.</typeparam>
        /// <param name="parameterName">Имя параметра.</param>
        /// <param name="defaultValue">Значение по умолчанию для параметра, 
        /// если он отсутствует в хранилище.</param>
        public T GetNoCache<T>(string parameterName, T defaultValue = default(T))
        {
            lock (lockObject)
            {
                object readed = null;
                ApplicationData.Current.LocalSettings.Values.TryGetValue(parameterName, out readed);
                if (readed != null)
                {
                    string json = readed.ToString();
                    T value = JsonConvert.DeserializeObject<T>(json);
                    return value;
                }
                else
                {
                    string json = JsonConvert.SerializeObject(defaultValue);
                    ApplicationData.Current.LocalSettings.Values[parameterName] = json;
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Удалить параметр из хранилища.
        /// </summary>
        /// <param name="parameterName">Имя параметра.</param>
        public void Remove(string parameterName)
        {
            lock (lockObject)
            {
                if (cache.ContainsKey(parameterName))
                    cache.Remove(parameterName);
                if (ContainsSetting(parameterName))
                    ApplicationData.Current.LocalSettings.Values.Remove(parameterName);
            }
        }

        /// <summary>
        /// Возвращает значение, содержится ли в хранилище настроек параметр
        /// с указанным именем.
        /// </summary>
        /// <param name="parameterName">Имя параметра.</param>
        public bool ContainsSetting(string parameterName)
        {
            return ApplicationData.Current.LocalSettings.Values.ContainsKey(parameterName);
        }
    }
}
