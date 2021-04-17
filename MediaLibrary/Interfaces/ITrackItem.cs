using System;
using System.Collections.Generic;
using System.Text;
using MediaLibrary.Enums;

namespace MediaLibrary.Interfaces
{
    public interface ITrackItem : IAudioItem
    {
        TrackType TrackType { get; }
        TimeSpan DurationTs { get; }
        IAudioItem? Group { get; }
        long? Playcount { get; }
        List<IAudioItem> Artists { get; }
    }

    public interface IAlbumTrack : ITrackItem
    {
        int Index { get; }
        DiscographyType AlbumType { get; }
    }
}
