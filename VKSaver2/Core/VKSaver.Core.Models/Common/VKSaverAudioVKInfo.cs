using Newtonsoft.Json;

namespace VKSaver.Core.Models.Common
{
    public class VKSaverAudioVKInfo
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("owner_id")]
        public int OwnerID { get; set; }

        [JsonProperty("album_id")]
        public int? AlbumID { get; set; }

        [JsonProperty("lyrics_id")]
        public int LyricsID { get; set; }
    }
}