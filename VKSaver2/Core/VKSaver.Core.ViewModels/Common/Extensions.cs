using ModernDev.InTouch;
using OneTeam.SDK.VK.Models.Audio;
using OneTeam.SDK.VK.Models.Docs;
using System.Collections.Generic;
using System.Linq;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Models.Transfer;
using static VKSaver.Core.Models.Common.FileContentTypeExtensions;

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

        public static IPlayerTrack ToPlayerTrack(this Audio audio)
        {
            return new PlayerTrack
            {
                Title = audio.Title,
                Artist = audio.Artist,
                Source = audio.Url,
                LyricsID = audio.LyricsId ?? 0
            };
        }

        public static IEnumerable<IPlayerTrack> ToPlayerTracks(this IEnumerable<VKAudio> tracks)
        {
            return tracks.Select(t => t.ToPlayerTrack());
        }

        public static IEnumerable<IPlayerTrack> ToPlayerTracks(this IEnumerable<Audio> tracks)
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

        public static IDownloadable ToDownloadable(this Audio audio)
        {
            return new SimpleDownloadable
            {
                ContentType = FileContentType.Music,
                Extension = ".mp3",
                FileName = audio.Title,
                Source = audio.Url
            };
        }

        public static IDownloadable ToDownloadable(this Doc document)
        {
            var downloadable = new SimpleDownloadable
            {
                Extension = $".{document.Ext}",
                FileName = document.Title,
                Source = document.Url
            };
            downloadable.ContentType = GetContentTypeFromExtension(downloadable.Extension);
            return downloadable;
        }
    }
}
