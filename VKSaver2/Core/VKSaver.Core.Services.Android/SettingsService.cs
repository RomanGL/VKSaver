using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Android.Content;
using Android.Preferences;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class SettingsService : ISettingsService
    {
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();
        private readonly object _lockObject = new object();
        private readonly ISharedPreferences _preferences;
        private readonly ISharedPreferencesEditor _preferencesEditor;

        public SettingsService(Context context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            
            _preferences = PreferenceManager.GetDefaultSharedPreferences(context);
            _preferencesEditor = _preferences.Edit();
        }

        /// <summary>
        /// Очистить хранилище настроек приложения.
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                _preferencesEditor.Clear();
                _preferencesEditor.Apply();
                _cache.Clear();
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

            lock (_lockObject)
            {
                _preferencesEditor.PutString(parameterName, json);
                _preferencesEditor.Apply();
                _cache[parameterName] = value;
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
            lock (_lockObject)
            {
                object readed = null;
                _cache.TryGetValue(parameterName, out readed);

                if (readed != null)
                    return (T)readed;

                readed = _preferences.GetString(parameterName, null);
                if (readed != null)
                {
                    string json = readed.ToString();
                    T value = JsonConvert.DeserializeObject<T>(json);

                    _cache[parameterName] = value;
                    return value;
                }
                else
                {
                    string json = JsonConvert.SerializeObject(defaultValue);
                    _preferencesEditor.PutString(parameterName, json);
                    _preferencesEditor.Apply();
                    _cache[parameterName] = defaultValue;
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
            lock (_lockObject)
            {
                object readed = null;
                readed = _preferences.GetString(parameterName, null);
                if (readed != null)
                {
                    string json = readed.ToString();
                    T value = JsonConvert.DeserializeObject<T>(json);
                    return value;
                }
                else
                {
                    string json = JsonConvert.SerializeObject(defaultValue);
                    _preferencesEditor.PutString(parameterName, json);
                    _preferencesEditor.Apply();
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
            lock (_lockObject)
            {
                _cache.Remove(parameterName);
                _preferencesEditor.Remove(parameterName);
                _preferencesEditor.Apply();
            }
        }

        /// <summary>
        /// Возвращает значение, содержится ли в хранилище настроек параметр
        /// с указанным именем.
        /// </summary>
        /// <param name="parameterName">Имя параметра.</param>
        public bool ContainsSetting(string parameterName)
        {
            return _preferences.Contains(parameterName);
        }
    }
}
