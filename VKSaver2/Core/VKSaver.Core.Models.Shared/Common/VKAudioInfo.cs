using System;

namespace VKSaver.Core.Models.Common
{
    public sealed class VKAudioInfo
    {
        public VKAudioInfo(int id, int ownerId, string title, string artist, string source, int duration)
            : this(id, ownerId)
        {
            Title = title;
            Artist = artist;
            Source = source;
            Duration = duration;
        }

        public VKAudioInfo(int id, int ownerId)
        {
            Id = id;
            OwnerId = ownerId;
        }

        public VKAudioInfo()
        {
        }

        public int Id { get; set; }
        public int OwnerId { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Source { get; set; }
        public int Duration { get; set; }
    }
}
