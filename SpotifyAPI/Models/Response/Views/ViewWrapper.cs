using System.Text.Json.Serialization;
using Newtonsoft.Json;
using SpotifyLibrary.Models.Response.Interfaces;
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
        public string Name { get; set; }
    }
}
