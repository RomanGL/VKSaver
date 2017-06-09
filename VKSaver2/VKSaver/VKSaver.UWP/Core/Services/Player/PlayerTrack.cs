using Newtonsoft.Json;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Player;

namespace VKSaver.Core.Services.Player
{
    internal sealed class PlayerTrack : IPlayerTrack
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("source")]
        public string Source { get; }

        [JsonProperty("vk_infoff")]
        public VKSaverAudioVKInfo VKInfo { get; set; }

        public bool Equals(IPlayerTrack other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return this.Title == other.Title &&
                this.Artist == other.Artist &&
                this.Source == other.Source;
        }
    }
}
