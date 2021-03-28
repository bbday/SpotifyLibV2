using System;
using System.Collections.Generic;
using JetBrains.Annotations;


namespace SpotifyLibrary.Models.Response.Interfaces
{
    public interface ITrackItem : IAudioItem
    {
        TimeSpan? DurationTs { get; }
        [CanBeNull] IAudioItem Group { get; }
        long? Playcount { get; }
        List<IAudioItem> Artists { get; }
    }
}
