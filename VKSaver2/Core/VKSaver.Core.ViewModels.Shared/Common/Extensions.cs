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
        public static IEnumerable<T> GetFromCentre<T>(this IList<T> source, int sourceIndex, int count, out int newIndex)
        {
            var newItems = new T[count];

            int needLeft = count / 2;
            int needRight = count - needLeft;

            int availableRight = source.Count - sourceIndex;
            if (availableRight < needRight)
            {
                needLeft += needRight - availableRight;
                needRight -= needRight - availableRight;
            }
            else if (sourceIndex < needLeft)
            {
                needRight += needLeft - sourceIndex;
                needLeft -= needLeft - sourceIndex;
            }

            int newPos = needLeft;
            for (int i = sourceIndex; i < sourceIndex + needRight; i++)
            {
                newItems[newPos] = source[i];
                newPos++;
            }

            newPos = 0;
            int offset = sourceIndex - needLeft;
            for (int i = sourceIndex - needLeft; i < needLeft + offset; i++)
            {
                newItems[newPos] = source[i];
                newPos++;
            }

            newIndex = needLeft;
            return newItems;
        }

        public static IPlayerTrack ToPlayerTrack(this Audio audio)
        {
            return new PlayerTrack
            {
                Title = audio.Title,
                Artist = audio.Artist,
                Source = audio.Url,
                VKInfo = new VKSaverAudioVKInfo
                {
                    ID = audio.Id,
                    OwnerID = audio.OwnerId,
                    AlbumID = audio.AlbumId,
                    LyricsID = audio.LyricsId ?? 0,
                    Genre = audio.GenreId
                }
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
                Source = audio.Url,
                Metadata = audio.ToVKSaverAudio()
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

        public static VKSaverAudio ToVKSaverAudio(this Audio audio)
        {
            return new VKSaverAudio
            {
                Track = new VKSaverAudioTrackInfo
                {
                    Title = audio.Title,
                    Artist = audio.Artist,
                    Encryption = AudioEncryptionMethod.vks0x0
                },
                VK = new VKSaverAudioVKInfo
                {
                    ID = audio.Id,
                    OwnerID = audio.OwnerId,
                    AlbumID = audio.AlbumId,
                    LyricsID = audio.LyricsId ?? 0,
                    Genre = audio.GenreId
                }
            };
        }

        public static List<T> GroupsToList<T>(this IEnumerable<JumpListGroup<T>> source)
        {
            return source.SelectMany(g => g).ToList();
        }
    }
}
