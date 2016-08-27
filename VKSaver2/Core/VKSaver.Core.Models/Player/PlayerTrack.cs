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
        
        [JsonProperty("vk_info")]
        public VKSaverAudioVKInfo VKInfo { get; set; }

        [JsonIgnore]
        public FileContentType ContentType { get { return FileContentType.Music; } }
        
        [JsonIgnore]
        public string Extension { get { return ".mp3"; } }
        
        [JsonIgnore]
        public string FileName
        {
            get
            {
                if (VKInfo != null)
                    return $"{VKInfo.OwnerID} {VKInfo.ID}";
                return Title;
            }
        }

        [JsonIgnore]
        public object Metadata { get { return this.ToVKSaverAudio(); } }

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
