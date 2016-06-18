using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKSaver.Design
{
    public sealed class TopTracksViewModelDesign
    {
        public object[] Tracks
        {
            get
            {
                return new[]
                {
                    new { Name = "Focus", Artist = new { Name = "Ariana Grande" }, PlayCount = 1967 },
                    new { Name = "Focus", Artist = new { Name = "Ariana Grande" }, PlayCount = 1967 },
                    new { Name = "Focus", Artist = new { Name = "Ariana Grande" }, PlayCount = 1967 },
                    new { Name = "Focus", Artist = new { Name = "Ariana Grande" }, PlayCount = 1967 },
                    new { Name = "Focus", Artist = new { Name = "Ariana Grande" }, PlayCount = 1967 },
                    new { Name = "Focus", Artist = new { Name = "Ariana Grande" }, PlayCount = 1967 },
                    new { Name = "Focus", Artist = new { Name = "Ariana Grande" }, PlayCount = 1967 }
                };
            }
        }
    }
}
