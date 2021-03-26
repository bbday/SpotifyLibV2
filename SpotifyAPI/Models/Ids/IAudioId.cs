using System;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Enums;

namespace SpotifyLibrary.Models.Ids
{
    public interface IAudioId : IEquatable<IAudioId>
    {
        AudioService IdType { get; }
        AudioType AudioType { get; }
    }
}
