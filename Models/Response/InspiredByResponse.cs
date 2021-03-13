using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SpotifyLibV2.Models.Response
{
    public class InspiredByResponse
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }
        [JsonPropertyName("mediaItems")]
        public List<MediaItem> MediaItems { get; set; }
    }

    public class MediaItem
    {
        [JsonPropertyName("uri")]
        public string Uri { get; set; }
    }
}
