using System;
using System.Collections.Generic;
using Spotify.Lib.Interfaces;

namespace Spotify.Lib.Models.TracksnEpisodes
{
    public interface ITrackItem : ISpotifyItem
    {
        TrackType TrackType { get; }
        TimeSpan DurationTs { get; }
        ISpotifyItem? Group { get; }
        long? Playcount { get; }
        List<ISpotifyItem> Artists { get; }
    }
}