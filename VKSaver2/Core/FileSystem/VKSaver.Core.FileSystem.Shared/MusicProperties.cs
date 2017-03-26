using System;
using System.Collections.Generic;

namespace VKSaver.Core.FileSystem
{
    public sealed class MusicProperties
    {
        internal MusicProperties()
        {
        }

        public string Album { get; internal set; }
        public string AlbumArtist { get; internal set; }
        public string Artist { get; internal set; }
        public string Bitrate { get; internal set; }
        public IList<string> Composers { get; internal set; }
        public IList<string> Conductors { get; internal set; }
        public TimeSpan Duration { get; internal set; }
        public IList<string> Genre { get; internal set; }
        public IList<string> Producers { get; internal set; }
        public string Publisher { get; internal set; }
        public uint Rating { get; internal set; }
        public string Subtitle { get; internal set; }
        public string Title { get; internal set; }
        public uint TrackNumber { get; internal set; }
        public IList<string> Writers { get; internal set; }
        public uint Year { get; internal set; }
    }
}
