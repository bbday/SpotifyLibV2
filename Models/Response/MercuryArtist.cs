using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SpotifyLibV2.Models.Response
{
    public class ArtistCover
    {
        /// <summary>
        /// https CDN Url.
        /// </summary>
        [JsonPropertyName("uri")]
        public string Uri { get; set; }
    }

    public class GenericSpotifyTrack : GenericSpotifyItem
    {
        /// <summary>
        /// Name of the track.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// If the track is explicit (contains vulgar language).
        /// </summary>
        [JsonPropertyName("explicit")]
        public bool Explicit { get; set; }
    }
    public class PlaycountedTrack : GenericSpotifyTrack
    {
        /// <summary>
        /// Number of plays. If the value is null then spotify displays "< 1000"
        /// </summary>
        [JsonPropertyName("playcount")]
        public long? Playcount { get; set; }
    }

    public class ArtistGenericRelease : GenericSpotifyItem
    {
        /// <summary>
        /// Name of the album
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Album Cover
        /// </summary>
        [JsonPropertyName("cover")]
        public ArtistCover Cover { get; set; }
    }

    public class MercuryArtist : GenericSpotifyItem
    {
        [JsonPropertyName("info")]
        public MercuryArtistShortInfo Info { get; set; }

        /// <summary>
        /// Header of Artist object, <see cref="ArtistHeaderImage"/>
        /// </summary>
        [JsonPropertyName("header_image")]
        public ArtistHeaderImage HeaderImage { get; set; }

        /// <summary>
        /// A top tracks object wrapper. See: <see cref="ArtistTopTracksObject"/>
        /// </summary>
        [JsonPropertyName("top_tracks")]
        public ArtistTopTracksObject TopTracks { get; set; }

        /// <summary>
        /// A top tracks object wrapper. See: <see cref="ArtistTopTracksObject"/>
        /// </summary>
        [JsonPropertyName("related_artists")]
        public ArtistRelatedArtistsObject RelatedArtists { get; set; }

        /// <summary>
        /// Artist's Discography
        /// </summary>
        [JsonPropertyName("releases")]
        public ArtistDiscographyObject Discography { get; set; }

        /// <summary>
        /// Artist's Merchandise
        /// </summary>
        [JsonPropertyName("merch")]
        public ArtistMerchanidseObject Merchandise { get; set; }

        /// <summary>
        /// The gallery (shown for example on the about page)
        /// </summary>
        [JsonPropertyName("gallery")]
        public ArtistGalleryObject Gallery { get; set; }

        /// <summary>
        /// Latest release of the artist.
        /// </summary>
        [JsonPropertyName("latest_release")]
        public ArtistDiscographyRelease LatestRelease { get; set; }

        [JsonPropertyName("pinned_item")]
        public PinnedItem PinnedItem { get; set; }

        [JsonPropertyName("creator_about")]
        public CreatorAbout CreatorAbout { get; set; }
    }
    public class CreatorAbout
    {
        [JsonPropertyName("monthlyListeners")]
        public long? MonthlyListeners { get; set; }

        [JsonPropertyName("globalChartPosition")]
        public long? GlobalChartPosition { get; set; }
    }

    public class PinnedItem
    {
        [JsonPropertyName("type")] public string Type { get; set; }

        [JsonPropertyName("uri")] public string Uri { get; set; }

        [JsonPropertyName("title")] public string Title { get; set; }

        [JsonPropertyName("image")] public string Image { get; set; }

        [JsonPropertyName("subtitle")] public string Subtitle { get; set; }

        [JsonPropertyName("secondsToExpiration")] public long? SecondsToExpiration { get; set; }
    }

    public class ArtistGalleryObject
    {

    }
    public class ArtistMerchanidseObject
    {
        [JsonPropertyName("items")]
        public IEnumerable<MerchItem> Items { get; set; }
    }

    public class MerchItem
    {
        /// <summary>
        /// Name of the item
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        /// <summary>
        /// Extra metadata.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }
        /// <summary>
        /// External link to the item.
        /// </summary>
        [JsonPropertyName("link")]
        public Uri Link { get; set; }
        /// <summary>
        /// Https cdn url of the image.
        /// </summary>
        [JsonPropertyName("image_uri")]
        public string ImageUrl { get; set; }
        /// <summary>
        /// Price of the item (In $USD.)
        /// </summary>
        [JsonPropertyName("price")]
        public string Price { get; set; }
    }
    public class MercuryArtistShortInfo
    {
        /// <summary>
        /// Name of the artist
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Name of the artist
        /// </summary>
        [JsonPropertyName("portraits")]
        public IEnumerable<ArtistCover> Portraits { get; set; }

        /// <summary>
        /// Boolean whether or not the artist is verified.
        /// </summary>
        [JsonPropertyName("verified")]
        public bool Verified { get; set; }
    }
    public class ArtistTopTracksObject
    {
        /// <summary>
        /// Max. 10 top tracks of the artist
        /// </summary>
        [JsonPropertyName("tracks")]
        public IEnumerable<ArtistTopTrack> Tracks { get; set; }
    }
    public class ArtistTopTrack : PlaycountedTrack
    {
        /// <summary>
        /// Album tied to the track. <see cref="TopTrackRelease"/>
        /// </summary>
        [JsonPropertyName("release")]
        public ArtistGenericRelease Release { get; set; }

    }
    public class ArtistRelatedArtistsObject
    {
        /// <summary>
        /// Max. 20 artists related to the artist.
        /// </summary>
        [JsonPropertyName("artists")]
        public IEnumerable<ArtistRelated> Artists { get; set; }
    }
    public class ArtistRelated : GenericSpotifyItem
    {
        private static readonly Regex InitialsRegex = new Regex(@"(\b[a-zA-Z])[a-zA-Z]* ?");
        private string _name;
        [System.Text.Json.Serialization.JsonIgnore]
        public string Initials { get; set; }

        /// <summary>
        /// Name of the artist.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                Initials = InitialsRegex.Replace(value, "$1");
            }
        }

        /// <summary>
        /// Spotify uri: spotify:track:XXXXXXXXX
        /// </summary>
        [JsonPropertyName("portraits")]
        public IEnumerable<ArtistCover> Portraits { get; set; }
    }
    public class ArtistHeaderImage
    {
        /// <summary>
        /// https CDN Url.
        /// </summary>
        [JsonPropertyName("image")]
        public string Image { get; set; }
        /// <summary>
        /// No clue yet what this is!
        /// </summary>
        [JsonPropertyName("offset")]
        public int Offset { get; set; }
    }
    public class ArtistDiscographyObject
    {
        /// <summary>
        /// Albums wrapper.
        /// </summary>
        [JsonPropertyName("albums")]
        public DiscographyWrapper Albums { get; set; }
        /// <summary>
        /// Singles wrapper.
        /// </summary>
        [JsonPropertyName("singles")]
        public DiscographyWrapper Singles { get; set; }
        /// <summary>
        /// AppearsOn wrapper.
        /// </summary>
        [JsonPropertyName("appears_on")]
        public DiscographyWrapper AppearsOn { get; set; }
        /// <summary>
        /// Compilations wrapper.
        /// </summary>
        [JsonPropertyName("compilations")]
        public DiscographyWrapper Compilations { get; set; }
    }
    public class DiscographyWrapper
    {
        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }
        [JsonPropertyName("releases")]
        public IEnumerable<ArtistDiscographyRelease> Releases { get; set; }
    }

    public class ArtistDiscographyRelease : ArtistGenericRelease
    {
        /// <summary>
        /// Year of (digital) release.
        /// </summary>
        [JsonPropertyName("year")]
        public int Year { get; set; }

        /// <summary>
        /// Month of (digital) release.
        /// </summary>
        [JsonPropertyName("month")]
        public int Month { get; set; }

        /// <summary>
        /// Day of (digital) release.
        [JsonPropertyName("day")]
        public int Day { get; set; }

        /// <summary>
        /// Number of tracks inside the album.
        /// </summary>
        [JsonPropertyName("track_count")]
        public int TrackCount { get; set; }

        /// <summary>
        /// Basically the tracks of the album/single. May be null!
        /// </summary>
        [JsonPropertyName("discs")]
        public IEnumerable<DiscographyDisc>? Discs { get; set; }
    }

    public class DiscographyDisc
    {
        /// <summary>
        /// Number of the disc. (starts at 1)
        /// </summary>
        [JsonPropertyName("number")]
        public int Number { get; set; }
        /// <summary>
        /// Name of the album (This is always null if you are fetching from artist)
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("tracks")]
        public IEnumerable<DiscographyTrack> Tracks { get; set; }
    }

    public class DiscographyTrack : PlaycountedTrack
    {
        /// <summary>
        /// Number from 0 to 100. See the spotify api for more details.
        /// </summary>
        [JsonPropertyName("popularity")]
        public int Popularity { get; set; }

        /// <summary>
        /// Hierarchical number of the track.
        /// </summary>
        [JsonPropertyName("number")]
        public int Number { get; set; }

        /// <summary>
        /// Duration in ms.
        /// </summary>
        [JsonPropertyName("duration")]
        public long Duration { get; set; }

        /// <summary>
        /// Boolean indicating whether or not the track is playable (region etc).
        /// </summary>
        [JsonPropertyName("playable")]
        public bool Playable { get; set; }

        /// <summary>
        /// List of simple artist objects. See <see cref="QuickArtist"/>
        /// </summary>
        [JsonPropertyName("artists")]
        public IEnumerable<QuickArtist> Artists { get; set; }
    }
    public class QuickArtist : GenericSpotifyItem
    {
        /// <summary>
        /// Name of the artist.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
