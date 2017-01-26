using Newtonsoft.Json;

namespace VKSaver.Core.Models
{
    public sealed class VKAdData
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("subtitle")]
        public string Subtitle { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("external_link")]
        public string ExternalLink { get; set; }

        [JsonProperty("external_name")]
        public string ExternalName { get; set; }

        [JsonProperty("ad_name")]
        public string AdName { get; set; }

        [JsonProperty("photo")]
        public string Photo { get; set; }
    }
}