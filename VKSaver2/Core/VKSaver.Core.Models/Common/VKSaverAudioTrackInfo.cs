using Newtonsoft.Json;

namespace VKSaver.Core.Models.Common
{
    public class VKSaverAudioTrackInfo
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("encryption")]
        public AudioEncryptionType Encryption { get; set; }

        [JsonProperty("sample_rate")]
        public int SampleRate { get; set; }

        [JsonProperty("channel_count")]
        public int ChannelCount { get; set; }

        [JsonProperty("bitrate")]
        public int EncodingBitrate { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }
    }
}
