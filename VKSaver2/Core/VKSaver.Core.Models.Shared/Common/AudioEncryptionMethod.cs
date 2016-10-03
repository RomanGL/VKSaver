namespace VKSaver.Core.Models.Common
{
    public enum AudioEncryptionMethod : byte
    {
        /// <summary>
        /// None. Шифрование отсутствует.
        /// </summary>
        vks0x0 = 0,
        /// <summary>
        /// Reverse. Шифрование инверсией байтов.
        /// </summary>
        vks0x1
    }
}
