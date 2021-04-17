using Newtonsoft.Json;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class FullAlbum : SimpleAlbum
    {
        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; } = default!;
        [JsonProperty("album_type")]
        public string AlbumType { get; set; } = default!;

        [JsonProperty("tracks")]
        public Paging<SimpleTrack> Tracks { get; set; } = default!;
    }
}
