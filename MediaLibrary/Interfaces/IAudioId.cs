using System;
using MediaLibrary.Enums;

namespace MediaLibrary.Interfaces
{
    public interface IAudioId : IEquatable<IAudioId>
    {
        AudioServiceType IdType { get; }
        AudioItemType AudioType { get; }
        string ToString();
    }
}
