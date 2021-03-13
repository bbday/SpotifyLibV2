using System;
using SpotifyLibV2.Enums;

namespace SpotifyLibV2.Ids
{
    public interface IAudioId : IEquatable<IAudioId>
    {
        AudioIdType IdType { get; }
        AudioType Type { get; }
    }
    public enum AudioIdType
    {
        Spotify,
        Local
    }
}
