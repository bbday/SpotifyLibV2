using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using SpotifyLibV2.Enums;
using SpotifyLibV2.Models.Shared;

namespace SpotifyLibV2.Models.Response
{
    public class ArtistHeaderedItem
    {

    }
    public class MercuryArtist : GenericSpotifyItem
    {
        private TopTracks _topTracks;
        private PinnedItem _pinnedItem;
        private Info _info;

        [JsonIgnore] public string Initials { get; set; }

        [JsonProperty("info")]
        public Info Info
        {
            get => _info;
            set
            {
                _info = value;
                Regex initials = new Regex(@"(\b[a-zA-Z])[a-zA-Z]* ?");
                Initials = initials.Replace(value.Name, "$1");
                if (PinnedItem != null)
                {
                    PinnedItem.ArtistInfo = value;
                }
            }
        }

        [JsonProperty("header_image")]
        public HeaderImage HeaderImage { get; set; }

        [JsonProperty("top_tracks")]
        public TopTracks TopTracks
        {
            get => _topTracks;
            set
            {
                value.Tracks.ForEach(k => k.ContextUri = Uri);
                var j = value.Tracks.Select((k, i) => (k, i))
                    .ToList();
                j.ForEach(k => k.k.ContextIndex = k.i + 1);
                value.Tracks = j.Select(z => z.k).ToList();
                _topTracks = value;
            }
        }

        [JsonProperty("upcoming_concerts")]
        public UpcomingConcerts UpcomingConcerts { get; set; }

        [JsonProperty("related_artists")]
        public RelatedArtists RelatedArtists { get; set; }

        [JsonProperty("biography")]
        public Biography Biography { get; set; }

        [JsonProperty("releases")]
        public Releases Releases { get; set; }

        [JsonProperty("merch")]
        public Merch Merch { get; set; }

        [JsonProperty("gallery")]
        public Gallery Gallery { get; set; }

        [JsonProperty("latest_release")]
        public LatestReleaseElement LatestRelease { get; set; }

        [JsonProperty("published_playlists")]
        public PublishedPlaylists PublishedPlaylists { get; set; }

        [JsonProperty("monthly_listeners")]
        public MonthlyListeners MonthlyListeners { get; set; }

        [JsonProperty("creator_about")]
        public CreatorAbout CreatorAbout { get; set; }

        [JsonProperty("pinned_item")]
        public PinnedItem PinnedItem
        {
            get => _pinnedItem;
            set
            {
                if (value != null)
                {
                    if (Info?.Portraits != null)
                    {
                        value.ArtistInfo = Info;
                    }

                    _pinnedItem = value;
                }
            }
        }

        [JsonProperty]
        public bool Saved { get; set; }
    }

    public class Biography
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class CreatorAbout
    {
        [JsonProperty("monthlyListeners")]
        public long? MonthlyListeners { get; set; }

        [JsonProperty("globalChartPosition")]
        public long? GlobalChartPosition { get; set; }
    }

    public class Gallery
    {
        [JsonProperty("images")]
        public List<SpotifyCover> Images { get; set; }
    }

    public class HeaderImage
    {
        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("offset")]
        public long? Offset { get; set; }
    }

    public class Info
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("portraits")]
        public List<SpotifyCover> Portraits { get; set; }

        [JsonProperty("verified")]
        public bool? Verified { get; set; }
    }

    public class LatestReleaseElement : ArtistHeaderedItem
    {
        public static string GetAbbreviatedFromFullName(int year, int month, int day)
        {
            var monthName = new DateTime(year, month, day)
                .ToString("MMM", CultureInfo.InvariantCulture);
            return monthName.ToUpper();
        }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cover")]
        public SpotifyCover Cover { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("track_count")]
        public int TrackCount { get; set; }

        [JsonProperty("month")]
        public int Month { get; set; }

        [JsonProperty("day")]
        public int Day { get; set; }

        [JsonIgnore] public string Description => $"{GetAbbreviatedFromFullName(Year, Month, Day)} {Day}, {Year}";
    }

    public class Merch
    {
        [JsonProperty("items")]
        public List<Item> Items { get; set; }
    }

    public class Item
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("link")]
        public Uri Link { get; set; }

        [JsonProperty("image_uri")]
        public Uri ImageUri { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }
    }

    public class MonthlyListeners
    {
        [JsonProperty("listener_count")]
        public long? ListenerCount { get; set; }
    }

    public class PinnedItem : ArtistHeaderedItem
    {
        private string _sub;
        private Info _info;

        [JsonIgnore]
        public Info ArtistInfo
        {
            get => _info;
            set
            {
                _info = value;
                PostedByString = $"Posted by {value.Name}";
            }
        }
        [JsonIgnore]
        public string PostedByString { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("subtitle")]
        public string Subtitle
        {
            get => _sub;
            set
            {
                _sub = value;
                if (value != null)
                {
                    PostedByString = value;
                }
            }
        }

        [JsonProperty("secondsToExpiration")]
        public long? SecondsToExpiration { get; set; }
    }

    public class PublishedPlaylists
    {
        [JsonProperty("playlists")]
        public List<Playlist> Playlists { get; set; }
    }

    public class Playlist : DiscographyItem
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cover")]
        public SpotifyCover Cover { get; set; }

        [JsonProperty("follower_count")]
        public long FollowerCount { get; set; }
    }

    public class RelatedArtists
    {
        [JsonProperty("artists")]
        public List<RelatedArtistsArtist> Artists { get; set; }
    }

    public class RelatedArtistsArtist : GenericSpotifyItem
    {
        private string _name;
        [JsonProperty("name")]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                Regex initials = new Regex(@"(\b[a-zA-Z])[a-zA-Z]* ?");
                Initials = initials.Replace(value, "$1");
            }
        }
        [JsonIgnore] public string Initials { get; set; }
        [JsonProperty("portraits")]
        public List<SpotifyCover> Portraits { get; set; }
    }

    public class Releases
    {
        [JsonProperty("albums")]
        public Albums Albums
        {
            get; set;
        }
        [JsonProperty("singles")]
        public Albums Singles
        {
            get; set;
        }

        [JsonProperty("appears_on")]
        public Albums AppearsOn
        {
            get; set;
        }
        [JsonProperty("compilations")]
        public Albums Compilations
        {
            get; set;
        }
    }

    public class Albums
    {
        [JsonProperty("releases")]
        public List<AlbumsRelease> Releases
        {
            get; set;
        }

        [JsonProperty("total_count")]
        public long? TotalCount { get; set; }
    }

    public class DiscographyItem : GenericSpotifyItem
    {
        [JsonIgnore]
        public string DerivedFrom { get; set; }

        [JsonIgnore]
        public DiscographyType DiscographyType { get; set; }
    }
    public class AlbumsRelease : DiscographyItem
    {
        private List<DiscTrack> _tracks;

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cover")]
        public SpotifyCover Cover { get; set; }

        [JsonProperty("year")]
        public long? Year { get; set; }

        [JsonProperty("track_count")]
        public long? TrackCount { get; set; }

        public delegate void InvokeDelegate(AlbumsRelease sender,
            List<DiscTrack> obj);
        public static InvokeDelegate InvokeMethod;

        [JsonProperty("discs",
            DefaultValueHandling = DefaultValueHandling.Include)]
        public List<Disc> Discs { get; set; }

        [JsonProperty("month")]
        public long? Month { get; set; }

        [JsonProperty("day")]
        public long? Day { get; set; }

    }

    public class Disc
    {
        [JsonProperty("number")]
        public int Number { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("tracks")]
        public List<DiscTrack> Tracks { get; set; }
    }

    public class DiscTrack : GenericSpotifyItem
    {
        private List<TrackArtist> _artists;
        private int _numb;
        [JsonIgnore]
        public int DiscNumber { get; set; }


        [JsonProperty("playcount")]
        public int Playcount { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonIgnore]
        public string ArtistsString { get; set; }

        [JsonProperty("popularity")]
        public int Popularity { get; set; }

        [JsonProperty("number")]
        public int Number
        {
            get => _numb;
            set
            {
                _numb = value;
                //  base.ContextIndex = value;
            }
        }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("explicit")]
        public bool Explicit { get; set; }

        [JsonProperty("playable")]
        public bool Playable { get; set; }

        [JsonProperty("artists")]
        public List<TrackArtist> Artists
        {
            get => _artists;
            set
            {
                _artists = value;
                ArtistsString = string.Join(", ", value.Select(z => z.Name));
            }
        }
    }

    public partial class TrackArtist
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }
    }

    public class TopTracks
    {
        [JsonProperty("tracks")]
        public List<TopTracksTrack> Tracks { get; set; }
    }

    public class TopTracksTrack : GenericSpotifyItem
    {
        [JsonProperty("playcount")]
        public long? Playcount { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("release")]
        public TrackRelease Release { get; set; }

        [JsonProperty("explicit")]
        public bool? Explicit { get; set; }
    }

    public class TrackRelease
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cover")]
        public SpotifyCover Cover { get; set; }
    }

    public class UpcomingConcerts
    {
        [JsonProperty("inactive_artist")]
        public bool? InactiveArtist { get; set; }
    }
}
