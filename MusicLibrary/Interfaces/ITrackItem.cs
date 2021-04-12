using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MusicLibrary.Enum;

namespace MusicLibrary.Interfaces
{
    public interface ITrackItem : IAudioItem
    {
        TrackType TrackType { get; }
        TimeSpan? DurationTs { get; }
        [CanBeNull] IAudioItem Group { get; }
        long? Playcount { get; }
        List<IAudioItem> Artists { get; }
    }

    public interface IAlbumTrack : ITrackItem
    {
        int Index { get; }
    }
}
