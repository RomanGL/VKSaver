using Newtonsoft.Json;
using System;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Models.Player
{
    public class CachedTrack : IPlayerTrack
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("vk_info")]
        public VKSaverAudioVKInfo VKInfo { get; set; }

        [JsonIgnore]
        public TimeSpan Duration { get; set; }

        [JsonIgnore]
        public VKSaverAudioFile File { get; set; }

        public bool Equals(IPlayerTrack other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return this.Title == other.Title &&
                this.Artist == other.Artist &&
                this.Source == other.Source;
        }

        public override string ToString()
        {
            return $"{Artist} - {Title}";
        }
    }
}
