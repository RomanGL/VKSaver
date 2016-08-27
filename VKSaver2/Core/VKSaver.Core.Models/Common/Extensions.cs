using VKSaver.Core.Models.Player;

namespace VKSaver.Core.Models.Common
{
    public static class Extensions
    {
        public static VKSaverAudio ToVKSaverAudio(this IPlayerTrack track)
        {
            return new VKSaverAudio
            {
                Track = new VKSaverAudioTrackInfo
                {
                    Title = track.Title,
                    Artist = track.Artist
                },
                VK = track.VKInfo
            };
        }
    }
}
