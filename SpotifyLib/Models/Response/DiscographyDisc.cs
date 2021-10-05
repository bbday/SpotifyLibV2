using System.Collections.Generic;
using System.Text.Json.Serialization;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models.Response
{
    public readonly struct DiscographyDisc
    {
        [JsonConstructor]
        public DiscographyDisc(int number, string? name, IEnumerable<DiscographyTrack> tracks)
        {
            Number = number;
            Name = name;
            Tracks = tracks;
        }

        /// <summary>
        /// Number of the disc. (starts at 1)
        /// </summary>
        [JsonPropertyName("number")]
        public int Number { get; }
        /// <summary>
        /// Name of the album (This is always null if you are fetching from artist)
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; }
        [JsonPropertyName("tracks")]
        public IEnumerable<DiscographyTrack> Tracks { get; }
    }

    public readonly struct DiscographyTrack
    {
        [JsonConstructor]
        public DiscographyTrack(SpotifyId uri, string name, Quick[] artists, bool playable, long duration, int number, int popularity, long? playcount, bool @explicit)
        {
            Uri = uri;
            Name = name;
            Artists = artists;
            Playable = playable;
            Duration = duration;
            Number = number;
            Popularity = popularity;
            Playcount = playcount;
            Explicit = @explicit;
        }

        /// <summary>
        /// Number from 0 to 100. See the spotify api for more details.
        /// </summary>
        [JsonPropertyName("popularity")]
        public int Popularity { get; }

        /// <summary>
        /// Hierarchical number of the track.
        /// </summary>
        [JsonPropertyName("number")]
        public int Number { get; }

        /// <summary>
        /// Duration in ms.
        /// </summary>
        [JsonPropertyName("duration")]
        public long Duration { get; }
        /// <summary>
        /// Boolean indicating whether or not the track is playable (region etc).
        /// </summary>
        [JsonPropertyName("playable")]
        public bool Playable { get; }

        /// <summary>
        /// List of simple artist objects. See <see cref="Quick"/>
        /// </summary>
        [JsonPropertyName("artists")]
        public Quick[] Artists { get; }

        [JsonPropertyName("playcount")]
        public long? Playcount { get; }
        [JsonPropertyName("name")]
        public string Name { get; }
        [JsonPropertyName("explicit")]
        public bool Explicit { get; }
        [JsonConverter(typeof(UriToSpotifyIdConverter))]
        public SpotifyId Uri { get; }
    }
}
