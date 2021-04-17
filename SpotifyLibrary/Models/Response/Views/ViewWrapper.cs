using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

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
        public List<ViewImage>? Images { get; set; }
    }

    public struct ViewImage
    {
        public string Name { get; set; }

        private string _mainUrl;
        public string Url
        {
            get => _mainUrl;
            set
            {
                if (value != null)
                    _mainUrl = value;
            }
        }

        public string Uri
        {
            get => _mainUrl;
            set
            {
                if (value != null)
                    _mainUrl = value;
            }
        }

        public int? Width { get; set; }
        public int? Height { get; set; }
    }
}
