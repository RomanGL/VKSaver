using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;

namespace VKSaver.Core.Models.Database
{
    [Table("Artists")]
    public class VKSaverArtist : IEquatable<VKSaverArtist>
    {
        [PrimaryKey]
        public string DbKey { get; set; }

        public string Name { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<VKSaverTrack> Tracks { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<VKSaverAlbum> Albums { get; set; }

        public bool Equals(VKSaverArtist other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return this.DbKey == other.DbKey;
        }
    }
}
