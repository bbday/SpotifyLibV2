using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SpotifyLibV2.Enums;
using SpotifyLibV2.Models.Response;
using SpotifyLibV2.Models.Response.ArtistMercuryInfo;
using J = System.Text.Json.Serialization.JsonPropertyNameAttribute;
using System.Linq;

namespace SpotifyLibV2.Models.Search
{
    public interface IHitBase
    {
      long Total { get; set; }
      int Count { get; }
    }
    public class GenericHit
    {
        [J("name")]
        public string Name { get; set; }

        [J("uri")]
        public string Uri { get; set; }
    }
    public class FullSearchResponse : ISearchResponse
    {
        [J("results")]
        public Results Results { get; set; }

        [J("requestId")]
        public string RequestId { get; set; }

        [J("categoriesOrder")]
        public List<string> CategoriesOrder { get; set; }

        public SearchType SearchType => SearchType.Full;
    }

    public class Results
    {
        [J("tracks")]
        public HitsObject<TracksHit> Tracks { get; set; }

        [J("albums")]
        public HitsObject<AlbumsHit> Albums { get; set; }

        [J("artists")]
        public HitsObject<ArtistHit> Artists { get; set; }

        [J("playlists")]
        public HitsObject<PlaylistHit> Playlists { get; set; }

        [J("profiles")]
        public HitsObject<ProfilesHit> Profiles { get; set; }

        [J("genres")]
        public HitsObject<GenreHit> Genres { get; set; }

        [J("topHit")]
        public HitsObject<TopHit> TopHit { get; set; }

        [J("shows")]
        public HitsObject<ShowsHit> Shows { get; set; }

        [J("audioepisodes")]
        public HitsObject<AudioepisodesHit> Audioepisodes { get; set; }

        [J("topRecommendations")]
        public HitsObject<PlaylistHit> TopRecommendations { get; set; }
    }

    public class HitsObject<T> : IHitBase where T : GenericHit
    {
        private List<T> _hits;
        [J("hits")]
        public List<T> Hits
        {
            get
            {
                return _hits;
            }
            set
            {
                _hits = value;
                //InsertedCount += value.Count;
            }
        }

        [J("total")]
        public long Total { get; set; }
        public int Count => Hits.Count;
    }

    public class PlaylistHit : GenericHit
    {
        [J("image")]
        public string Image { get; set; }

        [J("followersCount")]
        public int FollowersCount { get; set; }

        [J("author")]
        public string Author { get; set; }
    }
    public class ShowsHit: GenericHit
    {
        [J("image")]
        public string Image { get; set; }

        [J("showType")]
        public string ShowType { get; set; }

        [J("popularity")]
        public double? Popularity { get; set; }

        [J("musicAndTalk")]
        public bool MusicAndTalk { get; set; }
    }
    public class TracksHit  : GenericHit
    {

        [J("image")]
        public string Image { get; set; }

        [J("artists")]
        public List<GenericHit> Artists { get; set; }

        [J("album")]
        public GenericHit Album { get; set; }

        [J("duration")]
        public long Duration { get; set; }

        [J("mogef19")]
        public bool Mogef19 { get; set; }

        [J("popularity")]
        public double? Popularity { get; set; }

        [J("lyricsMatch")]
        public bool LyricsMatch { get; set; }
    }
    public class AlbumsHit : GenericHit
    {
        [J("image")]
        public string Image { get; set; }

        [J("artists")]
        public List<GenericHit> Artists { get; set; }
    }
    public class AudioepisodesHit : GenericHit
    {
        [J("image")]
        public string Image { get; set; }

        [J("explicit")]
        public bool Explicit { get; set; }

        [J("duration")]
        public long Duration { get; set; }

        [J("popularity")]
        public double? Popularity { get; set; }

        [J("musicAndTalk")]
        public bool MusicAndTalk { get; set; }
    }
    public class ArtistHit : GenericHit
    {
        [J("image")]
        public string Image { get; set; }
    }
    public class ProfilesHit : GenericHit
    {
        [J("image")]
        public string Image { get; set; }

        [J("followersCount")]
        public long? FollowersCount { get; set; }
    }

    public class GenreHit : GenericHit
    {
        [J("image")]
        public string Image { get; set; }
    }
    public class TopHit : GenericHit
    {
        [J("image")]
        public string Image { get; set; }
    }
}