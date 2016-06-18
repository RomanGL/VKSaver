using System.Threading.Tasks;

namespace VKSaver.Core.ViewModels.Collections
{
    /// <summary>
    /// Указывает на поддержку подгрузки элементов в начало и в конец списка.
    /// </summary>
    public interface ISupportUpDownIncrementalLoading
    {
        /// <summary>
        /// Имеются для еще элементы для подгрузки в начало списка.
        /// </summary>
        bool HasMoreUpItems { get; }
        /// <summary>
        /// Имеются ли еще элемнты для подгрузки в конец списка.
        /// </summary>
        bool HasMoreDownItems { get; }

        /// <summary>
        /// Подгрузить следующую порцию элементов в начало списка.
        /// </summary>
        /// <param name="count">Количество элементов для подгрузки.</param>
        Task<object> LoadUpAsync(uint count);
        /// <summary>
        /// Подгрузить следующую порцию элементов в конец списка.
        /// </summary>
        /// <param name="count">Количество элементов для подгрузки.</param>
        Task<object> LoadDownAsync(uint count);
    }
}
