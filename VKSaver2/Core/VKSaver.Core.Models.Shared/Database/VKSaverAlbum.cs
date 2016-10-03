using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;

namespace VKSaver.Core.Models.Database
{
    [Table("Albums")]
    public class VKSaverAlbum : IEquatable<VKSaverAlbum>
    {
        [PrimaryKey]
        public string DbKey { get; set; }

        public string Name { get; set; }

        [ForeignKey(typeof(VKSaverArtist))]
        public string ArtistName { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<VKSaverTrack> Tracks { get; set; }

        [ManyToOne]
        public VKSaverArtist Artist { get; set; }

        public bool Equals(VKSaverAlbum other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return this.DbKey == other.DbKey;
        }
    }
}
