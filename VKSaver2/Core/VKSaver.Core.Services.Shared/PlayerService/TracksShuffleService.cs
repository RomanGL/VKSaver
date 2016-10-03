using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class TracksShuffleService : ITracksShuffleService
    {
        public async Task ShuffleTracksAsync(IList<IPlayerTrack> tracks, int currentID)
        {
            await Task.Run(() => ShuffleTracks(tracks, currentID));
        }

        public void ShuffleTracks(IList<IPlayerTrack> tracks, int currentID)
        {
            IPlayerTrack temp = null;
            temp = tracks[0];
            tracks[0] = tracks[currentID];
            tracks[currentID] = temp;

            for (int i = 1; i < tracks.Count; i++)
            {
                int id = _random.Next(1, tracks.Count);
                if (id == i) continue;

                temp = tracks[i];
                tracks[i] = tracks[id];
                tracks[id] = temp;
            }
        }

        private readonly Random _random = new Random(Environment.TickCount);
    }
}
