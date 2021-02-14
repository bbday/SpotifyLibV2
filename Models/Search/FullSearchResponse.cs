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
    public class GenericHit : GenericSpotifyItem
    {
        public GenericHit(AudioType? typeOverride = null) : base(typeOverride)
        {

        }
    }
    public class MercurySearchResponse : ISearchResponse
    {
        [J("results")] public Results Results { get; set; }
        [J("requestId")] public string RequestId { get; set; }
        [J("categoriesOrder")] public List<string> CategoriesOrder { get; set; }
        public SearchType SearchType => SearchType.Full;
    }

    public class Results
    {
        [J("tracks")] public Tracks Tracks { get; set; }
        [J("albums")] public Albums Albums { get; set; }
        [J("artists")] public Artists Artists { get; set; }
        [J("playlists")] public Playlists Playlists { get; set; }
        [J("profiles")] public Profiles Profiles { get; set; }
        [J("genres")] public Genres Genres { get; set; }
        [J("topHit")] public TopHits TopHit { get; set; }
        [J("shows")] public Shows Shows { get; set; }
        [J("audioepisodes")] public Audioepisodes Audioepisodes { get; set; }
        [J("topRecommendations")] public TopRecommendations TopRecommendations { get; set; }
    }

    public class Albums
    {
        [J("hits")] public List<AlbumsHit> Hits { get; set; }
        [J("total")] public long Total { get; set; }
    }

    public class AlbumsHit : GenericHit
    {
        [J("name")] public string Name { get; set; }
        [J("image")] public string Image { get; set; }
        [J("artists")] public List<Album> Artists { get; set; }
        public string ArtissString => string.Join(",",
            Artists?.Select(z => z.Name) ?? Array.Empty<string>());
    }

    public partial class Album
    {
        [J("name")] public string Name { get; set; }
        [J("uri")] public string Uri { get; set; }
    }
    public class TopHits
    {
        [J("hits")] public List<TopHit> Hits { get; set; }
        [J("total")] public long Total { get; set; }
    }


    public class Artists
    {
        [J("hits")] public List<ArtistsHit> Hits { get; set; }
        [J("total")] public long Total { get; set; }
    }

    public class TopHit : GenericHit
    {
        [J("name")] public string Name { get; set; }
        private bool IsUser => string.IsNullOrEmpty(Author);
        [J("image")] public string Image { get; set; }
        [J("artists")] public List<Album> Artists { get; set; }
        [J("album")] public Album Album { get; set; }
        [J("duration")] public long Duration { get; set; }
        [J("mogef19")] public bool Mogef19 { get; set; }
        [J("lyricsMatch")] public bool LyricsMatch { get; set; }
        [J("followersCount")] public int followersCount { get; set; }
        [J("author")] public string Author { get; set; }
    }

    public partial class ArtistsHit : GenericHit
    { 
        private string _image;
        private string _name;
        [J("name")]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                var initials = new Regex(@"(\b[a-zA-Z])[a-zA-Z]* ?");
                Initials = initials.Replace(value, "$1");
            }
        }

        [J("image")]
        public string Image
        {
            get => _image ?? "ms-appx:///Assets/DefaultIcon.png";
            set
            {
                _image = value;
            }
        }

        public string Initials
        {
            get;
            set;
        }
    }

    public class Audioepisodes
    {
        [J("hits")] public List<AudioepisodesHit> Hits { get; set; }
        [J("total")] public long Total { get; set; }
    }

    public class AudioepisodesHit : GenericHit
    {
        [J("name")] public string Name { get; set; }
        [J("image")] public string Image { get; set; }
        [J("explicit")] public bool Explicit { get; set; }
        [J("duration")] public long Duration { get; set; }
        [J("musicAndTalk")] public bool MusicAndTalk { get; set; }
    }

    public class Genres
    {
        [J("hits")] public List<object> Hits { get; set; }
        [J("total")] public long Total { get; set; }
    }

    public class Playlists
    {
        [J("hits")] public List<PlaylistsHit> Hits { get; set; }
        [J("total")] public long Total { get; set; }
    }

    public class PlaylistsHit : GenericHit
    {
        public PlaylistsHit() : base(AudioType.Playlist)
        {

        }
        [J("name")] public string Name { get; set; }
        [J("image")] public string Image { get; set; }
        [J("followersCount")] public long FollowersCount { get; set; }
        [J("author")] public string Author { get; set; }
    }

    public class Profiles
    {
        [J("hits")] public List<ProfilesHit> Hits { get; set; }
        [J("total")] public long Total { get; set; }
    }

    public class ProfilesHit : GenericHit
    {
        [J("name")] public string Name { get; set; }
        [J("followersCount")] public long FollowersCount { get; set; }
        [J("image")] public Uri Image { get; set; }
    }

    public class Shows
    {
        [J("hits")] public List<ShowsHit> Hits { get; set; }
        [J("total")] public long Total { get; set; }
    }

    public class ShowsHit : GenericHit
    {
        [J("name")] public string Name { get; set; }
        [J("image")] public string Image { get; set; }
        [J("showType")] public string ShowType { get; set; }
        [J("musicAndTalk")] public bool MusicAndTalk { get; set; }
    }

    public class TopRecommendations
    {
        [J("hits")] public List<TopRecommendationsHit> Hits { get; set; }
        [J("title")] public string Title { get; set; }
    }

    public class TopRecommendationsHit : GenericHit
    {
        [J("name")] public string Name { get; set; }
        [J("image")] public string Image { get; set; }
        [J("followersCount")] public int FollowersCount { get; set; }
        [J("author")] public string Author { get; set; }
    }

    public class Tracks
    {
        [J("hits")] public List<TracksHit> Hits { get; set; }
        [J("total")] public int Total { get; set; }
    }

    public class TracksHit : GenericHit
    {
        public string ArtissString => string.Join(",",
        Artists?.Select(z => z.Name) ?? Array.Empty<string>());
        [J("name")] public string Name { get; set; }
        [J("image")] public string Image { get; set; }
        [J("artists")] public List<Album> Artists { get; set; }
        [J("album")] public Album Album { get; set; }
        [J("duration")] public int Duration { get; set; }
        [J("lyricsMatch")] public bool? LyricsMatch { get; set; }
    }
}