using System.Collections.Generic;
using System.Text.Json.Serialization;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models.Response
{
    public readonly struct MercuryAlbum
    {
        [JsonConstructor]
        public MercuryAlbum(SpotifyId uri, string name, MercuryCover cover, string type, string label, ushort trackCount, string[] copyrights, Quick[] artists, AlbumDisc[] discs, AdditionalReleases? additional, AlbumsRelatedObject? related, ushort? year, ushort? month, ushort? day)
        {
            Uri = uri;
            Name = name;
            Cover = cover;
            Type = type;
            Label = label;
            TrackCount = trackCount;
            Copyrights = copyrights;
            Artists = artists;
            Discs = discs;
            Additional = additional;
            Related = related;
            Year = year;
            Month = month;
            Day = day;
        }

        [JsonPropertyName("uri")]
        [JsonConverter(typeof(UriToSpotifyIdConverter))]
        public SpotifyId Uri { get; }
        public string Name { get; }
        public MercuryCover Cover { get; }
        public ushort? Year { get; }
        public ushort? Month { get; }
        public ushort? Day { get; }
        public string Type { get; }
        public string Label { get; }
        [JsonPropertyName("track_count")]
        public ushort TrackCount { get; }
        public string[] Copyrights { get; }
        public Quick[] Artists { get; }
        public AlbumDisc[] Discs { get; }
        public AdditionalReleases? Additional { get; }
        public AlbumsRelatedObject? Related { get; }
    }

    public readonly struct AlbumDisc
    {
        [JsonConstructor]
        public AlbumDisc(AlbumTrack[] tracks, string name, ushort number)
        {
            Tracks = tracks;
            Name = name;
            Number = number;
        }

        public ushort Number { get; }
        public string Name { get; }
        public AlbumTrack[] Tracks { get; }
    }

    public struct AlbumTrack
    {
        [JsonConstructor]
        public AlbumTrack(SpotifyId uri, string name, ushort number, double duration, Quick[] artists, bool playable, bool @explicit, short popularity, long? playcount)
        {
            Uri = uri;
            Name = name;
            Number = number;
            Duration = duration;
            Artists = artists;
            Playable = playable;
            Explicit = @explicit;
            Popularity = popularity;
            Playcount = playcount;
        }

        [JsonPropertyName("uri")]
        [JsonConverter(typeof(UriToSpotifyIdConverter))]
        public SpotifyId Uri { get; }
        public long? Playcount { get; }
        public string Name { get; }
        public short Popularity { get; }
        public ushort Number { get; }
        public double Duration { get; }
        public bool Explicit { get; }
        public bool Playable { get; }
        public Quick[] Artists { get; }
    }
    public readonly struct AdditionalReleases
    {
        [JsonConstructor]
        public AdditionalReleases(IEnumerable<MercuryRelease> releases)
        {
            Releases = releases;
        }

        public IEnumerable<MercuryRelease> Releases { get; }
    }
    public readonly struct AlbumsRelatedObject
    {
        [JsonConstructor]
        public AlbumsRelatedObject(IEnumerable<MercuryRelease> releases)
        {
            Releases = releases;
        }

        public IEnumerable<MercuryRelease> Releases { get; }
    }
}
