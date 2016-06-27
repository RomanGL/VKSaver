using ModernDev.InTouch;
using PropertyChanged;

namespace VKSaver.Core.ViewModels.Common
{
    [ImplementPropertyChanged]
    public sealed class VKAudioWithImage
    {
        [DependsOn(nameof(VKTrack))]
        public string Title { get { return VKTrack.Title.Trim(); } }

        [DependsOn(nameof(VKTrack))]
        public string Artist { get { return VKTrack.Artist.Trim(); } }
        
        public string ImageURL { get; set; }
        
        public Audio VKTrack { get; set; }
    }
}
