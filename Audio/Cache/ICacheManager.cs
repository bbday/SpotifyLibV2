using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SpotifyLibV2.Ids;

namespace SpotifyLibV2.Audio.Cdn
{
    public interface ICacheManager
    {
        Task<(byte[]? chunk, bool exists)> TryGetChunk(IPlayableId id, int index);
        Task SaveChunk(IPlayableId id, int index, byte[] data);
        Task<T> GetItem<T>(string key, string subkey);
        Task SaveItem<T>(string key, string subkey, T data);
    }
}
