using System.Collections.Generic;
using System.Threading.Tasks;
using VKSaver.Core.Models.Player;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IPlayerPlaylistService
    {
        Task<bool> WritePlaylist(IEnumerable<IPlayerTrack> tracks);
        Task<bool> WriteShuffledPlaylist(IEnumerable<IPlayerTrack> tracks);
        Task<IEnumerable<IPlayerTrack>> ReadPlaylist();
        Task<IEnumerable<IPlayerTrack>> ReadShuffledPlaylist();
    }
}
