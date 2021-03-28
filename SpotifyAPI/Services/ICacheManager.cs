using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SpotifyLibrary.Models.Ids;

namespace SpotifyLibrary.Services
{
    public interface ICacheManager
    {
        bool AllowCacheOfKey(string key);
        Task<(byte[]? chunk, bool exists)> TryGetChunk(ISpotifyId id, int index);

        bool TryGetItem<T>(string key, out T result);
        void SaveItem<T>(string key, T item);
    }
}
