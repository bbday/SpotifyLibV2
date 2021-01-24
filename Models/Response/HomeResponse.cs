using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SpotifyLibV2.Models.Shared;

namespace SpotifyLibV2.Models.Response
{
    public class HomeResponse
    {
        [JsonPropertyName("content")] public HomeResponseContent Content { get; set; }

        [JsonPropertyName("external_urls")] public object ExternalUrls { get; set; }

        [JsonPropertyName("href")] public Uri Href { get; set; }

        [JsonPropertyName("id")] public string Id { get; set; }

        [JsonPropertyName("images")] public IEnumerable<SpotifyImage> Images { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("rendering")] public string Rendering { get; set; }

        [JsonPropertyName("tag_line")] public string TagLine { get; set; }

        [JsonPropertyName("type")] public string Type { get; set; }
    }

    public partial class HomeResponseContent
    {
        [JsonPropertyName("href")] public Uri Href { get; set; }

        [JsonPropertyName("items")] public IEnumerable<PurpleItem> Items { get; set; }

        [JsonPropertyName("limit")] public long? Limit { get; set; }

        [JsonPropertyName("next")] public object Next { get; set; }

        [JsonPropertyName("offset")] public long? Offset { get; set; }

        [JsonPropertyName("previous")] public object Previous { get; set; }

        [JsonPropertyName("total")] public long? Total { get; set; }
    }

    public partial class PurpleItem
    {
        [JsonPropertyName("content")] public ItemContent Content { get; set; }


        [JsonPropertyName("external_urls")] public object ExternalUrls { get; set; }

        [JsonPropertyName("href")] public Uri Href { get; set; }

        [JsonPropertyName("id")] public string Id { get; set; }

        [JsonPropertyName("images")] public List<SpotifyImage> Images { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("rendering")] public string Rendering { get; set; }

        [JsonPropertyName("tag_line")] public string TagLine { get; set; }

        [JsonPropertyName("type")] public string Type { get; set; }
    }

    public partial class ItemContent
    {
        [JsonPropertyName("href")] public Uri Href { get; set; }

        [JsonPropertyName("items")] public FluffyItem[] Items { get; set; }

        [JsonPropertyName("limit")] public long? Limit { get; set; }

        [JsonPropertyName("next")] public Uri Next { get; set; }

        [JsonPropertyName("offset")] public long? Offset { get; set; }

        [JsonPropertyName("previous")] public object Previous { get; set; }

        [JsonPropertyName("total")] public long? Total { get; set; }
    }

    public class FluffyItem : GenericSpotifyItem
    {
        private string _desc;

        [JsonPropertyName("collaborative")] public bool? Collaborative { get; set; }

        [JsonPropertyName("description")]
        public string Description
        {
            get => _desc;
            set { _desc = value; }
        }
        [JsonPropertyName("album_type")]
        public string AlbumType { get; set; }
        [JsonPropertyName("href")] public Uri Href { get; set; }

        [JsonPropertyName("images")] public List<SpotifyImage> Images { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("artists")]
        public Artist[] Artists { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public string GroupName { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string GroupTagLine { get; set; }
    }

    public class Artist : GenericSpotifyItem
    {
        [JsonPropertyName("href")]
        public Uri Href { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}

