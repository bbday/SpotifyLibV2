using System.Collections.Generic;
using System.Text.Json.Serialization;
using MediaLibrary;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;
using SpotifyLibrary.Ids;
using SpotifyLibrary.Interfaces;

namespace SpotifyLibrary.Models.Response.Mercury
{
    public class MercuryArtistInsights
    {
        /// <summary>
        /// Hex representation of the Gid of the artist
        /// </summary>
        public string ArtistGid { get; set; }
        /// <summary>
        /// Name of the artist
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Artist Profile Picture
        /// </summary>
        public string MainImageUrl { get; set; }

        public UrlImage HeaderImage { get; set; }

        /// <summary>
        /// Biography posted by the artist themselves. 
        /// </summary>
        public AutoBiographyObject AutoBiography { get; set; }

        /// <summary>
        /// Biography posted by someone else
        /// </summary>
        public string Biography { get; set; }

        /// <summary>
        /// Gallery Images
        /// </summary>
        public List<UrlImage> Images { get; set; }


        public int ImagesCount { get; set; }
        /// <summary>
        /// 0 if no chart position
        /// </summary>
        public int GlobalChartPosition { get; set; }

        /// <summary>
        /// Monthly Listeners
        /// </summary>
        public long MonthlyListeners { get; set; }

        /// <summary>
        /// Change (delta) in monthly listeners. Can be negative
        /// </summary>
        public long MonthlyListenersDelta { get; set; }

        /// <summary>
        /// Number of followers.
        /// </summary>
        public long FollowerCount { get; set; }
        public long FollowingCount { get; set; }

        public ArtistPlaylistsObject Playlists { get; set; }
        public List<ArtistCity> Cities { get; set; }

        public bool ContainsAutobiography => !string.IsNullOrEmpty(AutoBiography?.Body);
    }

    public class AutoBiographyObject
    {
        public string Body { get; set; }
        public Dictionary<string, string> Links { get; set; }
    }
    public class ArtistPlaylistsObject
    {
        public List<PublishedPlaylist> Entries { get; set; }
    }
    public class PublishedPlaylist : ISpotifyItem
    {
        private IAudioId _id;
        private List<UrlImage> _images;
        public AudioServiceType AudioService => AudioServiceType.Spotify;
        public AudioItemType Type => AudioItemType.Playlist;
        public IAudioId Id => _id ??= new PlaylistId(Uri);

        public List<UrlImage> Images => _images ??= new List<UrlImage>
        {
            new UrlImage
            {
                Url = ImageUrl
            }
        };
        [JsonPropertyName("name")]
        public string Name { get; set; }
        public string Uri { get; set; }
        public OwnerObject Owner { get; set; }
        public int Listeners { get; set; }
        public string Description => Owner.Name;

        public string ImageUrl { get; set; }
    }
    public class OwnerObject : ISpotifyItem
    {
        private IAudioId _id;
        public AudioServiceType AudioService => AudioServiceType.Spotify;
        public IAudioId Id => _id ??= new UserId(Uri);
        public AudioItemType Type => AudioItemType.User;
        public string Name { get; set; }
        public string Uri { get; set; }

        public string Description => "User";
        public List<UrlImage> Images { get; }
    }

    public class ArtistCity
    {
        /// <summary>
        /// 2 Letter country code
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// Unknown.
        /// </summary>
        public string Region { get; set; }
        /// <summary>
        /// String of the city
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// Number of monthly listeners
        /// </summary>
        public long Listeners { get; set; }

    }
}
