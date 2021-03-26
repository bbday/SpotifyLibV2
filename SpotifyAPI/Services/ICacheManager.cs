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
        Task SaveChunk(ISpotifyId id, int index, byte[] data);
        Task<T> GetItem<T>(string key, string subkey);
        Task SaveItem<T>(string key, string subkey, T data);
        T GetKey<T>(string key);
        void SaveKey<T>(string key, T data);
    }
}
