using System;

namespace SpotifyLibV2.Ids
{
    public interface IAudioId : IEquatable<IAudioId>
    {
        AudioIdType IdType { get; }
    }
    public enum AudioIdType
    {
        Spotify
    }
}
