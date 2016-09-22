﻿using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;

namespace VKSaver.Core.Models.Database
{
    [Table("Artists")]
    public class VKSaverArtist
    {
        [PrimaryKey]
        public string DbKey { get; set; }

        public string Name { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<VKSaverTrack> Tracks { get; set; }
    }
}
