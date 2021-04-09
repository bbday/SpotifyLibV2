using System.Collections.Generic;
using JetBrains.Annotations;
using MusicLibrary.Models;
using Newtonsoft.Json;
using SpotifyLibrary.Models.Response.Paging;

namespace SpotifyLibrary.Models.Response.Views
{
    public class ViewWrapper<T>
    {
        [JsonProperty("custom_fields")]
        public object CustomFields { get; set; }
        [JsonProperty("href")]
        public string Href { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("content")]
        public Paging<T> Content { get; set; }

        [JsonProperty("tag_line")]
        public string TagLine { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("rendering")]
        public string Rendering { get; set; }
        [JsonProperty("images")]
        [CanBeNull] public List<ViewImage> Images { get; set; }
    }

    public class ViewImage : UrlImage
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
