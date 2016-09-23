using Newtonsoft.Json;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using VKSaver.Core.Models.Database;

namespace VKSaver.Core.Models.Common
{
    [Table("VKInfo")]
    public class VKSaverAudioVKInfo
    {
        [JsonIgnore]
        [PrimaryKey]
        public string DbKey { get; set; }

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