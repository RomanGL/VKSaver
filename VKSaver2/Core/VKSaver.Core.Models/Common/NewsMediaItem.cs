using ModernDev.InTouch;
using Newtonsoft.Json;

namespace VKSaver.Core.Models.Common
{
    public class NewsMediaItem
    {
        [JsonProperty("audio")]
        public Audio Audio { get; set; }

        [JsonProperty("video")]
        public Video Video { get; set; }
    }
}
