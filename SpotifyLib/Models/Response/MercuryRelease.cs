using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models.Response
{
    public readonly struct MercuryShortRelease : ISpotifyItem
    {
        [JsonConstructor]
        public MercuryShortRelease(MercuryCover cover, string name, SpotifyId uri)
        {
            Cover = cover;
            Name = name;
            Uri = uri;
        }

        /// <summary>
        /// Name of the album
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; }
        /// <summary>
        /// Album Cover
        /// </summary>
        [JsonPropertyName("cover")]
        public MercuryCover Cover { get; }

        [JsonPropertyName("uri")]
        [JsonConverter(typeof(UriToSpotifyIdConverter))]
        public SpotifyId Uri { get; }
    }

    public readonly struct MercuryReleaseAsPlaylist : ISpotifyItem
    {
        [JsonConstructor]
        public MercuryReleaseAsPlaylist(string name, MercuryCover cover, SpotifyId uri, int followerCount)
        {
            Name = name;
            Cover = cover;
            Uri = uri;
            FollowerCount = followerCount;
        }

        /// <summary>
        /// Name of the album
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; }

        /// <summary>
        /// Album Cover
        /// </summary>
        [JsonPropertyName("cover")]
        public MercuryCover Cover { get; }

        [JsonPropertyName("uri")]
        [JsonConverter(typeof(UriToSpotifyIdConverter))]
        public SpotifyId Uri { get; }

        /// <summary>
        /// Year of (digital) release.
        /// </summary>
        [JsonPropertyName("follower_count")]
        public int FollowerCount { get; }
    }
    public readonly struct MercuryRelease : ISpotifyItem
    {
        [JsonConstructor]
        public MercuryRelease(string name, MercuryCover cover, SpotifyId uri, int year, short? month, short? day, int trackCount,
            IEnumerable<DiscographyDisc>? discs)
        {
            Name = name;
            Cover = cover;
            Uri = uri;
            Year = year;
            Month = month;
            Day = day;
            TrackCount = trackCount;
            Discs = discs;
        }

        /// <summary>
        /// Name of the album
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; }

        /// <summary>
        /// Album Cover
        /// </summary>
        [JsonPropertyName("cover")]
        public MercuryCover Cover { get; }

        [JsonPropertyName("uri")]
        [JsonConverter(typeof(UriToSpotifyIdConverter))]
        public SpotifyId Uri { get; }

        /// <summary>
        /// Year of (digital) release.
        /// </summary>
        [JsonPropertyName("year")]
        public int Year { get; }

        /// <summary>
        /// Month of (digital) release.
        /// </summary>
        [JsonPropertyName("month")]
        public short? Month { get; }

        /// <summary>
        /// Day of (digital) release.
        [JsonPropertyName("day")]
        public short? Day { get; }

        /// <summary>
        /// Number of tracks inside the album.
        /// </summary>
        [JsonPropertyName("track_count")]
        public int TrackCount { get; }

        /// <summary>
        /// Basically the tracks of the album/single. May be null!
        /// </summary>
        [JsonPropertyName("discs")]
        public IEnumerable<DiscographyDisc>? Discs { get; }

        public DateTime ReleaseDateAsDateTime => new DateTime(Year == 0 ? 2021 : Year, Month ?? 1, Day ?? 1);
    }
}
