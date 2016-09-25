using Newtonsoft.Json;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Player;

namespace VKSaver.Core.Models.Database
{
    [Table("Tracks")]
    public class VKSaverTrack : IPlayerTrack
    {
        [PrimaryKey]
        public string Source { get; set; }

        public string Title { get; set; }

        [ForeignKey(typeof(VKSaverArtist))]
        public string Artist { get; set; }

        [ForeignKey(typeof(VKSaverAudioVKInfo)), JsonIgnore]
        public string VKInfoKey { get; set; }

        [ForeignKey(typeof(VKSaverAlbum)), JsonIgnore]
        public string AlbumKey { get; set; }

        [ForeignKey(typeof(VKSaverGenre)), JsonIgnore]
        public string GenreKey { get; set; }

        [ForeignKey(typeof(VKSaverFolder)), JsonIgnore]
        public string FolderKey { get; set; }

        [JsonIgnore]
        public TimeSpan Duration { get; set; }

        [OneToOne]
        public VKSaverAudioVKInfo VKInfo { get; set; }
        
        [ManyToOne, JsonIgnore]
        public VKSaverArtist AppArtist { get; set; }

        [ManyToOne, JsonIgnore]
        public VKSaverAlbum AppAlbum { get; set; }

        [ManyToOne, JsonIgnore]
        public VKSaverGenre AppGenre { get; set; }

        [ManyToOne, JsonIgnore]
        public VKSaverFolder AppFolder { get; set; }

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
