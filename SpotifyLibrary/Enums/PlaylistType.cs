using System;
using MediaLibrary;

namespace SpotifyLibrary.Enums
{
    public enum PlaylistType
    {
        [String("PLAYLIST")] UserPlaylist,
        [String("COLLABORATIVE")] CollaborativePlaylist,
        [String("CHART")] ChartedList,
        [String("A PLAYLIST MADE FOR {0}")] MadeForUser,
        [String("RADIO")] Radio,
        [String("SEEDED")] SeededRadio,
        [String("LIKED SONGS")] Collection,
    }
}
