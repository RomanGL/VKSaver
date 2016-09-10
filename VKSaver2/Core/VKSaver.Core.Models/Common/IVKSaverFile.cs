using System;
using System.IO;
using System.Threading.Tasks;

namespace VKSaver.Core.Models.Common
{
    /// <summary>
    /// Представляет файл VKSaver.
    /// </summary>
    /// <typeparam name="T">Тип метаданных в файле.</typeparam>
    public interface IVKSaverFile<T> : IDisposable
    {
        Task<Stream> GetContentStreamAsync();
        Task<T> GetMetadataAsync();
    }
}
