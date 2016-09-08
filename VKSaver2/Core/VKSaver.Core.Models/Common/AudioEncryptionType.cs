using Newtonsoft.Json;

namespace VKSaver.Core.Models.Common
{
    public enum AudioEncryptionType : byte
    {
        [JsonProperty("vks0x0")]
        None = 0,
        [JsonProperty("vks0x1")]
        Reverse
    }
}
