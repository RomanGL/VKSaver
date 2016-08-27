using Newtonsoft.Json;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Models.Transfer
{
    public interface IDownloadable
    {
        /// <summary>
        /// Тип содержимого файла.
        /// </summary>
        [JsonIgnore]
        FileContentType ContentType { get; }
        /// <summary>
        /// Расширение файла.
        /// </summary>
        [JsonIgnore]
        string Extension { get; }
        /// <summary>
        /// Путь к файлу для загрузки.
        /// </summary>
        [JsonProperty("source")]
        string Source { get; }
        /// <summary>
        /// Имя файла, которое будет применено к результирующему файлу.
        /// </summary>
        [JsonIgnore]
        string FileName { get; }

        /// <summary>
        /// Метаданные к файлу.
        /// </summary>
        [JsonIgnore]
        object Metadata { get; }
    }
}
