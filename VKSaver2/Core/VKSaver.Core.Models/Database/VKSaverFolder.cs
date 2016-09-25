using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;

namespace VKSaver.Core.Models.Database
{
    [Table("Folders")]
    public class VKSaverFolder
    {
        [PrimaryKey]
        public string Path { get; set; }

        public string Name { get; set; }       
        
        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<VKSaverTrack> Tracks { get; set; }
    }
}
