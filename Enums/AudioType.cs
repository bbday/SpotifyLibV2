using SpotifyLibV2.Attributes;

namespace SpotifyLibV2.Enums
{
    public enum AudioType
    {
        Unknown,
        Track,
        Artist,
        Show,
        Playlist,
        Profile,
        Album,
        Episode,
        Image,
        Local,
        Link,
        Station,
        TopRecommendation,
        RelatedArtist,
    }

    public enum PlaylistType
    {
        [String("PLAYLIST")] UserPlaylist,
        [String("CHART")] ChartedList,
        [String("A PLAYLIST MADE FOR {0}")] MadeForUser,
        [String("RADIO")] Radio,
        [String("SEEDED")] SeededRadio,
        [String("LIKED SONGS")] Collection,
    }
}