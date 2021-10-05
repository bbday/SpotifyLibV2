using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models.Response
{
    public readonly struct MercuryArtist
    {
        [JsonConstructor]
        public MercuryArtist(MercuryArtistShortInfo info, ArtistHeaderImage headerImage, ArtistTopTracksObject topTracks, 
            ArtistReleasesObject? releases, ArtistRelatedArtistsObject relatedArtists, CreatorAbout creatorAbout,
            MercuryRelease? latestRelease, ArtistsPlaylistsWrapper playlists, string uri)
        {
            Info = info;
            HeaderImage = headerImage;
            TopTracks = topTracks;
            Releases = releases;
            RelatedArtists = relatedArtists;
            CreatorAbout = creatorAbout;
            LatestRelease = latestRelease;
            Playlists = playlists;
            Uri = uri;
        }
        [JsonPropertyName("uri")]
        public string Uri { get; }
        [JsonPropertyName("info")]
        public MercuryArtistShortInfo Info { get; }

        /// <summary>
        /// Header of Artist object, <see cref="ArtistHeaderImage"/>
        /// </summary>
        [JsonPropertyName("header_image")]
        public ArtistHeaderImage HeaderImage { get; }

        /// <summary>
        /// A top tracks object wrapper. See: <see cref="ArtistTopTracksObject"/>
        /// </summary>
        [JsonPropertyName("top_tracks")]
        public ArtistTopTracksObject TopTracks { get; }

        public ArtistReleasesObject? Releases { get; }
        [JsonPropertyName("related_artists")]
        public ArtistRelatedArtistsObject RelatedArtists { get; }
        [JsonPropertyName("creator_about")]
        public CreatorAbout CreatorAbout { get; }

        /// <summary>
        /// Latest release of the artist.
        /// </summary>
        [JsonPropertyName("latest_release")]
        public MercuryRelease? LatestRelease { get; }

        [JsonPropertyName("published_playlists")]
        public ArtistsPlaylistsWrapper Playlists { get; }

    }
    public readonly struct ArtistsPlaylistsWrapper
    {
        [JsonConstructor]
        public ArtistsPlaylistsWrapper(List<MercuryRelease> playlists)
        {
            Playlists = playlists;
        }

        [JsonPropertyName("playlists")]
        public List<MercuryRelease> Playlists { get;  }
    }

    public readonly struct CreatorAbout
    {
        [JsonConstructor]
        public CreatorAbout(long? monthlyListeners, long? globalChartPosition)
        {
            MonthlyListeners = monthlyListeners;
            GlobalChartPosition = globalChartPosition;
        }

        [JsonPropertyName("monthlyListeners")]
        public long? MonthlyListeners { get; }

        [JsonPropertyName("globalChartPosition")]
        public long? GlobalChartPosition { get; }
    }
    public readonly struct ArtistRelatedArtistsObject
    {
        [JsonConstructor]
        public ArtistRelatedArtistsObject(IEnumerable<ArtistRelated> artists)
        {
            Artists = artists;
        }

        /// <summary>
        /// Max. 20 artists related to the artist.
        /// </summary>
        [JsonPropertyName("artists")]
        public IEnumerable<ArtistRelated> Artists { get; }
    }

    public readonly struct ArtistRelated : ISpotifyItem
    {
        [JsonConstructor]
        public ArtistRelated(SpotifyId uri, List<MercuryCover>? portraits, string name)
        {
            Uri = uri;
            Portraits = portraits;
            Name = name;
        }
        [JsonPropertyName("portraits")]
        public List<MercuryCover> Portraits { get; }
        [JsonPropertyName("uri")]
        [JsonConverter(typeof(UriToSpotifyIdConverter))]
        public SpotifyId Uri { get; }
        [JsonPropertyName("name")]
        public string Name { get; }

        public string Image => Portraits?.FirstOrDefault().Uri ?? "ms-appx:///Assets/AlbumPlaceholder.png";
    }
    public readonly struct ArtistReleasesObject
    {
        [JsonConstructor]
        public ArtistReleasesObject(ArtistReleaseObject albums, ArtistReleaseObject singles, ArtistReleaseObject appearsOn, ArtistReleaseObject compilations)
        {
            Albums = albums;
            Singles = singles;
            AppearsOn = appearsOn;
            Compilations = compilations;
        }

        public ArtistReleaseObject Albums { get; }
        public ArtistReleaseObject Singles { get; }
        public ArtistReleaseObject AppearsOn { get; }
        public ArtistReleaseObject Compilations { get; }

    }
    public readonly struct ArtistReleaseObject
    {
        [JsonConstructor]
        public ArtistReleaseObject(int totalCount, IEnumerable<MercuryRelease>? releases)
        {
            TotalCount = totalCount;
            Releases = releases;
        }

        [JsonPropertyName("total_count")]
        public int TotalCount { get; }
        public IEnumerable<MercuryRelease>? Releases { get; }
        public bool HasReleases => Releases != null;
    }

    public readonly struct MercuryArtistShortInfo
    {
        [JsonConstructor]
        public MercuryArtistShortInfo(string name, bool verified, List<MercuryCover> portraits)
        {
            Name = name;
            Verified = verified;
            Portraits = portraits;
        }

        /// <summary>
        /// Name of the artist
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; }
        /// <summary>
        /// Boolean whether or not the artist is verified.
        /// </summary>
        [JsonPropertyName("verified")]
        public bool Verified { get; }

        /// <summary>
        /// Name of the artist
        /// </summary>
        [JsonPropertyName("portraits")]
        public List<MercuryCover> Portraits { get; }

    }
    public readonly struct ArtistHeaderImage
    {
        [JsonConstructor]
        public ArtistHeaderImage(string image, int offset)
        {
            Image = image;
            Offset = offset;
        }

        /// <summary>
        /// https CDN Url.
        /// </summary>
        [JsonPropertyName("image")]
        public string Image { get; }
        /// <summary>
        /// No clue yet what this is!
        /// </summary>
        [JsonPropertyName("offset")]
        public int Offset { get; }
    }
    public readonly struct ArtistTopTracksObject
    {
        [JsonConstructor]
        public ArtistTopTracksObject(IEnumerable<ArtistTopTrack> tracks)
        {
            Tracks = tracks;
        }

        /// <summary>
        /// Max. 10 top tracks of the artist
        /// </summary>
        [JsonPropertyName("tracks")]
        public IEnumerable<ArtistTopTrack> Tracks { get; }
    }
    public readonly struct ArtistTopTrack : ISpotifyItem
    {
        [JsonConstructor]
        public ArtistTopTrack(MercuryShortRelease release, SpotifyId uri, string name, long? playcount)
        {
            Release = release;
            Uri = uri;
            Name = name;
            Playcount = playcount;
        }

        /// <summary>
        /// Album tied to the track. <see cref="TopTrackRelease"/>
        /// </summary>
        [JsonPropertyName("release")]
        public MercuryShortRelease Release { get; }
        [JsonPropertyName("uri")]
        [JsonConverter(typeof(UriToSpotifyIdConverter))]
        public SpotifyId Uri { get; }
        [JsonPropertyName("name")]
        public string Name { get; }
        [JsonPropertyName("playcount")]
        public long? Playcount { get; }
    }
}
