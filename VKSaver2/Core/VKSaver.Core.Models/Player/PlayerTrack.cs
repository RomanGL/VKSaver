using System;
using Newtonsoft.Json;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Transfer;

namespace VKSaver.Core.Models.Player
{
    public class PlayerTrack : IPlayerTrack, IDownloadable
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("lyrics_id")]
        public long LyricsID { get; set; }
        
        [JsonIgnore]
        public FileContentType ContentType { get { return FileContentType.Music; } }
        
        [JsonIgnore]
        public string Extension { get { return ".mp3"; } }
        
        [JsonIgnore]
        public string FileName { get { return Title; } }

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
