using ModernDev.InTouch;
using System.Collections.Generic;
using System.Linq;
using VKSaver.Core.LinksExtractor;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Models.Transfer;
using static VKSaver.Core.Models.Common.FileContentTypeExtensions;

namespace VKSaver.Core.ViewModels.Common
{
    public static class Extensions
    {
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

        public static IEnumerable<IPlayerTrack> ToPlayerTracks(this IEnumerable<Audio> tracks)
        {
            return tracks.Select(t => t.ToPlayerTrack());
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

        public static IDownloadable ToDownloadable(this IVideoLink link, string title)
        {
            return new SimpleDownloadable
            {
                Extension = ".mp4",
                FileName = title,
                Source = link.Source,
                ContentType = FileContentType.Video
            };
        }
    }
}
