using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models.Ids;
using Spotify.Lib.Models.Response.SpotItems.SimpleItems;
using Spotify.Lib.Models.Response.SpotItems.Views;

namespace Spotify.Lib.Models.Response.SpotItems.FullItems
{

    public struct FullAlbum : ISpotifyItem
    {
        public string Uri { get; set; }
        public AudioItemType Type => AudioItemType.Album;
        public ISpotifyId Id => new AlbumId(Uri);
        public string Name { get; set; }

        public string Description => string.Join(", ",
            Artists.Select(z => z.Name));

        public string Caption => AlbumType.ToString();
        public List<UrlImage> Images { get; set; }
        public List<SimpleArtist> Artists { get; set; }
        [JsonProperty("release_date")] public string ReleaseDate { get; set; }

        [JsonProperty("release_date_precision")]
        public string ReleaseDatePrecision { get; set; }

        [JsonProperty("total_tracks")] public short TotalTracks { get; set; }
        [JsonProperty("album_type")] public AlbumType AlbumType { get; set; }
        public Paging<SimpleTrack> Tracks { get; set; }
    }
}