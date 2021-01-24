using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SpotifyLibV2.Models.Shared
{
    public class SpotifyCover
    {
        [JsonProperty("uri")]
        [JsonPropertyName("uri")]
        public string Uri { get; set; }
    }
}