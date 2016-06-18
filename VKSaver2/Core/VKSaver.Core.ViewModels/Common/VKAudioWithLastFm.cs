using OneTeam.SDK.LastFm.Models.Audio;
using OneTeam.SDK.VK.Models.Audio;
using PropertyChanged;
using System.Linq;

namespace VKSaver.Core.ViewModels.Common
{
    [ImplementPropertyChanged]
    public sealed class VKAudioWithLastFm
    {
        [DependsOn(nameof(VKTrack))]
        public string Name { get { return VKTrack.Title.Trim(); } }

        [DependsOn(nameof(VKTrack))]
        public string Artist { get { return VKTrack.Artist.Trim(); } }

        [DependsOn(nameof(LFTrack))]
        public string ImageURL { get { return LFTrack?.Images?.LastOrDefault()?.URL; } }
        
        public VKAudio VKTrack { get; set; }
        
        public LFAudioBase LFTrack { get; set; }
    }
}
