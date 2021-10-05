using System.Collections.Generic;
using System.Text.Json.Serialization;
using AudioPlayerSpotify.Uwp.Models;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models.Response.SimpleItems
{
    public readonly struct SimpleAlbum : ISpotifyItem
    {
        [JsonConstructor]
        public SimpleAlbum(string name, List<UrlImage> images, 
            List<SimpleArtist> artists,
            string releaseDate, 
            string releaseDatePrecision, short totalTracks, AlbumType albumType,
            SpotifyId uri)
        {
            Name = name;
            Images = images;
            Artists = artists;
            ReleaseDate = releaseDate;
            ReleaseDatePrecision = releaseDatePrecision;
            TotalTracks = totalTracks;
            AlbumType = albumType;
            Uri = uri;
        }
        [JsonConverter(typeof(UriToSpotifyIdConverter))]
        public SpotifyId Uri { get; }
        public string Name { get; }
        public List<UrlImage> Images { get;}
        public List<SimpleArtist> Artists { get; }
        [JsonPropertyName("release_date")] 
        public string ReleaseDate { get; }

        [JsonPropertyName("release_date_precision")]
        public string ReleaseDatePrecision { get; }

        [JsonPropertyName("total_tracks")] 
        public short TotalTracks { get;  }
        [JsonPropertyName("album_type")] 
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AlbumType AlbumType { get; }
    }
}
