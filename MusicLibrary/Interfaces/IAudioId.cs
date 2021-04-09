using System;
using MusicLibrary.Enum;

namespace MusicLibrary.Interfaces
{
    public interface IAudioId : IEquatable<IAudioId>
    {
        AudioService IdType { get; }
        AudioType AudioType { get; }
    }
}

