using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SpotifyLibV2.Models.Search
{
    public class SearchItem : GenericSpotifyItem
    {
        public override string ToString() => Name;

        [JsonPropertyName("name")] public string Name { get; set; }


        [JsonPropertyName("image")] public string Image { get; set; }

        [JsonPropertyName("album")] public SearchAlbum Album { get; set; }

        [JsonPropertyName("artists")] public List<SearchAlbum> Artists { get; set; }

        [JsonPropertyName("fromLyrics")] public bool? FromLyrics { get; set; }

        [JsonPropertyName("explicit")] public bool? Explicit { get; set; }

        [JsonPropertyName("followers")] public long? Followers { get; set; }

    }
    public class SearchAlbum
    {
        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("uri")] public string Uri { get; set; }
    }
}