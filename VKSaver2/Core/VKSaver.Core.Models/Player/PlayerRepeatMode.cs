namespace VKSaver.Core.Models.Player
{
    /// <summary>
    /// Режим повтора проигрывателя.
    /// </summary>
    public enum PlayerRepeatMode : byte
    {
        /// <summary>
        /// Выключен.
        /// </summary>
        Disabled = 0,
        /// <summary>
        /// Повтор текущего трека.
        /// </summary>
        One,
        /// <summary>
        /// Повтор плейлиста.
        /// </summary>
        All
    }
}
