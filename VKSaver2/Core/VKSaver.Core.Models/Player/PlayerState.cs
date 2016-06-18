namespace VKSaver.Core.Models.Player
{
    /// <summary>
    /// Перечисление состояний проигрывателя.
    /// </summary>
    public enum PlayerState : byte
    {
        /// <summary>
        /// Проигрыватель мультимедиа закрыт.
        /// </summary>
        Closed = 0,
        /// <summary>
        /// Проигрыватель мультимедиа открывается.
        /// </summary>
        Opening = 1,
        /// <summary>
        /// Проигрыватель мультимедиа буферизует содержимое.
        /// </summary>
        Buffering = 2,
        /// <summary>
        /// Проигрыватель мультимедиа воспроизводит мультимедиа.
        /// </summary>
        Playing = 3,
        /// <summary>
        /// Проигрыватель мультимедиа приостановлен.
        /// </summary>
        Paused = 4,
        /// <summary>
        /// Проигрыватель мультимедиа остановлен.
        /// </summary>
        Stopped = 5
    }
}
