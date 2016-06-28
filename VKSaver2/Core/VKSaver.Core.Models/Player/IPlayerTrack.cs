using Newtonsoft.Json;
using System;
using VKSaver.Core.Models.Transfer;

namespace VKSaver.Core.Models.Player
{
    public interface IPlayerTrack : IEquatable<IPlayerTrack>
    {
        [JsonProperty("title")]
        string Title { get; set; }

        [JsonProperty("artist")]
        string Artist { get; set; }

        [JsonProperty("source")]
        string Source { get; }

        [JsonProperty("lyrics_id")]
        long LyricsID { get; set; }
    }
}
