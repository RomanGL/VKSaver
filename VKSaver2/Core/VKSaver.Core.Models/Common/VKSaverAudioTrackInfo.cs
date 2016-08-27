using Newtonsoft.Json;

namespace VKSaver.Core.Models.Common
{
    public class VKSaverAudioTrackInfo
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }
    }
}
