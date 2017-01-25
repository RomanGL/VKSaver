using Newtonsoft.Json;
using SQLite.Net.Attributes;

namespace VKSaver.Core.Models.Common
{
    public class VKSaverAudioTrackInfo
    {
        [JsonIgnore]
        [PrimaryKey]
        public string DbKey { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("encryption")]
        public AudioEncryptionMethod Encryption { get; set; }

        [JsonProperty("sample_rate")]
        public uint SampleRate { get; set; }

        [JsonProperty("channel_count")]
        public uint ChannelCount { get; set; }

        [JsonProperty("bitrate")]
        public uint EncodingBitrate { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }
    }
}
