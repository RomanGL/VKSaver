using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKSaver.Design
{
    public sealed class ArtistInfoViewModelDesign
    {
        public object Artist
        {
            get
            {
                return new
                {
                    Name = "Ariana Grande",
                    MegaImage = new { URL = "http://img2-ak.lst.fm/i/u/770x0/e7174af6fc0e9109afa826b709adb887.jpg" }
                };
            }
        }

        public object[] Tracks
        {
            get
            {
                return new[]
                {
                    new { Name = "Focus", Artist = new { Name = "Ariana Grande" } },
                    new { Name = "Focus", Artist = new { Name = "Ariana Grande" } },
                    new { Name = "Focus", Artist = new { Name = "Ariana Grande" } },
                    new { Name = "Focus", Artist = new { Name = "Ariana Grande" } },
                    new { Name = "Focus", Artist = new { Name = "Ariana Grande" } },
                    new { Name = "Focus", Artist = new { Name = "Ariana Grande" } },
                    new { Name = "Focus", Artist = new { Name = "Ariana Grande" } }
                };
            }
        }

        public object[] Albums
        {
            get
            {
                return new[]
                {
                    new { Name = "Problem" },
                    new { Name = "My Everything (Deluxe)"},
                    new { Name = "Focus Album" },
                    new { Name = "Born This Way" },
                    new { Name = "ARTPOP" }
                };
            }
        }

        public object[] Similar
        {
            get
            {
                return new[]
                {
                    new { Name = "Lady Gaga" },
                    new { Name = "Katy Perry" },
                    new { Name = "Kanye West" },
                    new { Name = "Eminem" }
                };
            }
        }
    }
}
