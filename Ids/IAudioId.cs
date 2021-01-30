namespace SpotifyLibV2.Ids
{
    public interface IAudioId
    {
        AudioIdType IdType { get; }
    }
    public enum AudioIdType
    {
        Spotify
    }
}
