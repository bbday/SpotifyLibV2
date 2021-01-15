using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SpotifyLibV2.Models.Shared;

namespace SpotifyLibV2.Models.Response
{
    public class HomeResponse
    {
        [JsonProperty("content")] public HomeResponseContent Content { get; set; }

        [JsonProperty("external_urls")] public object ExternalUrls { get; set; }

        [JsonProperty("href")] public Uri Href { get; set; }

        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("images")] public List<SpotifyImage> Images { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("rendering")] public string Rendering { get; set; }

        [JsonProperty("tag_line")] public string TagLine { get; set; }

        [JsonProperty("type")] public string Type { get; set; }
    }

    public partial class HomeResponseContent
    {
        [JsonProperty("href")] public Uri Href { get; set; }

        [JsonProperty("items")] public List<PurpleItem> Items { get; set; }

        [JsonProperty("limit")] public long? Limit { get; set; }

        [JsonProperty("next")] public object Next { get; set; }

        [JsonProperty("offset")] public long? Offset { get; set; }

        [JsonProperty("previous")] public object Previous { get; set; }

        [JsonProperty("total")] public long? Total { get; set; }
    }

    public partial class PurpleItem
    {
        [JsonProperty("content")] public ItemContent Content { get; set; }


        [JsonProperty("external_urls")] public object ExternalUrls { get; set; }

        [JsonProperty("href")] public Uri Href { get; set; }

        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("images")] public List<SpotifyImage> Images { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("rendering")] public string Rendering { get; set; }

        [JsonProperty("tag_line")] public string TagLine { get; set; }

        [JsonProperty("type")] public string Type { get; set; }
    }

    public partial class ItemContent
    {
        [JsonProperty("href")] public Uri Href { get; set; }

        [JsonProperty("items")] public FluffyItem[] Items { get; set; }

        [JsonProperty("limit")] public long? Limit { get; set; }

        [JsonProperty("next")] public Uri Next { get; set; }

        [JsonProperty("offset")] public long? Offset { get; set; }

        [JsonProperty("previous")] public object Previous { get; set; }

        [JsonProperty("total")] public long? Total { get; set; }
    }

    public class FluffyItem : GenericSpotifyItem
    {
        private string _desc;

        [JsonProperty("collaborative")] public bool? Collaborative { get; set; }

        [JsonProperty("description")]
        public string Description
        {
            get => _desc;
            set { _desc = value; }
        }
        [JsonProperty("album_type")]
        public string AlbumType { get; set; }
        [JsonProperty("href")] public Uri Href { get; set; }

        [JsonProperty("images")] public List<SpotifyImage> Images { get; set; }

        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("artists")]
        public Artist[] Artists { get; set; }

        [JsonIgnore]
        public string GroupName { get; set; }
        [JsonIgnore]
        public string GroupTagLine { get; set; }
    }

    public class Artist : GenericSpotifyItem
    {
        [JsonProperty("href")]
        public Uri Href { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}

