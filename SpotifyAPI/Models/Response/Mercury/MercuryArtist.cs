using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using MusicLibrary.Models;
using SpotifyLibrary.Models.Ids;

namespace SpotifyLibrary.Models.Response.Mercury
{
    public class MercuryCover
    {
        /// <summary>
        /// https CDN Url.
        /// </summary>
        [JsonPropertyName("uri")]
        public string Uri { get; set; }
    }

    public class GenericSpotifyTrack : ISpotifyItem
    {
        private IAudioId _id;

        public AudioService AudioService => AudioService.Spotify;
        [Newtonsoft.Json.JsonIgnore] 
        public IAudioId Id => _id ??= new TrackId(Uri);
        public virtual AudioType Type => AudioType.Track;

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

        public string Uri { get; set; }

        public virtual List<UrlImage> Images { get; set; }
        public virtual string Description { get; set; }
    }
    public class MercuryArtist : ISpotifyItem
    {
        private IAudioId _id;
        private List<UrlImage> _images;

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
        public MercuryRelease LatestRelease { get; set; }

        [JsonPropertyName("pinned_item")]
        public PinnedItem PinnedItem { get; set; }

        [JsonPropertyName("creator_about")]
        public CreatorAbout CreatorAbout { get; set; }

        [JsonPropertyName("published_playlists")]
        public ArtistsPlaylistsWrapper Playlists { get; set; }

        public AudioService AudioService => AudioService.Spotify;
        public IAudioId Id => _id ??= new ArtistId(Uri);
        [JsonPropertyName("uri")]
        public string Uri { get; set; }

        public string Description => $"{CreatorAbout.MonthlyListeners} followers";
        public string Name => Info.Name;
        public List<UrlImage> Images => Info.Portraits?.Select(z => new UrlImage
        {
            Url = z.Uri
        })?.ToList();
        public AudioType Type => AudioType.Artist;
    }

    public class ArtistsPlaylistsWrapper
    {
        [JsonPropertyName("playlists")]
        public IEnumerable<ArtistPlaylist> Playlists { get; set; }
    }

    public class ArtistPlaylist : MercuryGenericRelease
    {
        [JsonPropertyName(("follower_count"))]
        public int FollowerCount { get; set; }

        public override string Description => $"{FollowerCount} followers";
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

        [JsonPropertyName("comment")]
        [CanBeNull] public string Comment { get; set; }
        [JsonPropertyName("backgroundImage")]
        [CanBeNull]
        public string BackgroundImage { get; set; }
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
        public string Link { get; set; }
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
        public IEnumerable<MercuryCover> Portraits { get; set; }

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
    public class ArtistTopTrack : PlaycountedTrack, ITrackItem
    {
        private List<UrlImage> _images;
        /// <summary>
        /// Album tied to the track. <see cref="TopTrackRelease"/>
        /// </summary>
        [JsonPropertyName("release")]
        public MercuryGenericRelease Release { get; set; }

        public override List<UrlImage> Images
        {
            get
            {
                if(_images == null)
                {
                    _images = new List<UrlImage>(1)
                    {
                        new UrlImage
                        {
                            Url = Release.Cover.Uri
                        }
                    };
                }

                return _images;
            }
        }

        public TrackType TrackType => TrackType.AlbumTrack;
        public TimeSpan? DurationTs => TimeSpan.Zero;
        public IAudioItem Group => null;
        public List<IAudioItem> Artists { get; }
    }
    public class ArtistRelatedArtistsObject
    {
        /// <summary>
        /// Max. 20 artists related to the artist.
        /// </summary>
        [JsonPropertyName("artists")]
        public IEnumerable<ArtistRelated> Artists { get; set; }
    }
    public class ArtistRelated : IAudioItem
    {
        private static readonly Regex InitialsRegex = new(@"(\b[a-zA-Z])[a-zA-Z]* ?");
        private string _initials;

        public string Initials => _initials ??= InitialsRegex.Replace(Name, "$1");
        private IAudioId _id;
        /// <summary>
        /// Name of the artist.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Spotify uri: spotify:track:XXXXXXXXX
        /// </summary>
        [JsonPropertyName("portraits")]
        public IEnumerable<MercuryCover> Portraits { get; set; }

        public AudioService AudioService => AudioService.Spotify;
        public IAudioId Id => _id ??= new ArtistId(Uri);
        public string Uri { get; set; }

        public string Description => "Related Artist";
        public List<UrlImage> Images => Portraits?.Select(z => new UrlImage
        {
            Url = z.Uri
        })?.ToList();

        public AudioType Type => AudioType.Artist;
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
        public IEnumerable<MercuryRelease> Releases { get; set; }
    }

    public class QuickArtist : ISpotifyItem
    {
        private IAudioId _id;
        public AudioService AudioService => AudioService.Spotify;
        public IAudioId Id => _id ??= new ArtistId(Uri);
        public AudioType Type => AudioType.Artist;

        /// <summary>
        /// Name of the artist.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        public string Uri { get; set; }

        public string Description => throw new NotImplementedException("Nothing to describe for Discography track artist.");

        public List<UrlImage> Images =>
            throw new NotImplementedException("Discography track artist contains no images.");
    }
}
