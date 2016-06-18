using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKSaver.Design
{
    public sealed class PlayerViewModelDesign
    {
        public TimeSpan DurationStart { get { return TimeSpan.FromSeconds(57); } }

        public object CurrentTrack
        {
            get
            {
                return new
                {
                    Title = "Poker Face",
                    Artist = "Lady Gaga"
                };
            }
        }
    }
}
