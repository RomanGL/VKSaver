using Newtonsoft.Json;

namespace VKSaver.Core.Models.Common
{
    public class VKSaverAudio
    {
        [JsonProperty("app")]
        public string AppName { get; set; } = "VKSaver 2";

        [JsonProperty("track")]
        public VKSaverAudioTrackInfo Track { get; set; }

        [JsonProperty("vk")]
        public VKSaverAudioVKInfo VK { get; set; }
    }
}
