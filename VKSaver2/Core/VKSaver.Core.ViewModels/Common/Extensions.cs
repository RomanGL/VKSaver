using OneTeam.SDK.VK.Models.Audio;
using OneTeam.SDK.VK.Models.Docs;
using System.Collections.Generic;
using System.Linq;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Models.Transfer;

namespace VKSaver.Core.ViewModels.Common
{
    public static class Extensions
    {
        public static IPlayerTrack ToPlayerTrack(this VKAudio audio)
        {
            return new PlayerTrack
            {
                Title = audio.Title,
                Artist = audio.Artist,
                Source = audio.Source,
                LyricsID = audio.LyricsID
            };
        }

        public static IEnumerable<IPlayerTrack> ToPlayerTracks(this IEnumerable<VKAudio> tracks)
        {
            return tracks.Select(t => t.ToPlayerTrack());
        }

        public static IDownloadable ToDownloadable(this VKAudio audio)
        {
            return new SimpleDownloadable
            {
                ContentType = FileContentType.Music,
                Extension = ".mp3",
                FileName = audio.Title,
                Source = audio.Source
            };
        }

        public static IDownloadable ToDownloadable(this VKDocument document)
        {
            return new SimpleDownloadable
            {
                ContentType = FileContentType.Other,
                Extension = $".{document.Extension}",
                FileName = document.Title,
                Source = document.Url
            };
        }
    }
}
