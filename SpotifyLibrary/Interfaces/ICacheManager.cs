namespace SpotifyLibrary.Interfaces
{
    public interface ICacheManager
    {
        bool AllowCacheOfKey(string key);
        bool TryGetItem<T>(string key, out T result);
        void SaveItem<T>(string key, T item);
    }
}
