using System.Collections.Generic;
using System.Threading.Tasks;
using VKSaver.Core.Models.Player;

namespace VKSaver.Core.Services.Interfaces
{
    public interface ITracksShuffleService
    {
        Task ShuffleTracksAsync(IList<IPlayerTrack> tracks, int currentID);
        void ShuffleTracks(IList<IPlayerTrack> tracks, int currentID);
    }
}
