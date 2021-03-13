using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SpotifyLibV2.Models.Response.MercuryContext
{
    public class MercuryContextWrapperResponse
    {
        [JsonPropertyName("metadata")]
        public Metadata Metadata { get; set; }

        [JsonPropertyName("pages")]
        public object Pages { get; set; }

        [JsonPropertyName("uri")]
        public string Uri { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    public class Metadata
    {
        [JsonPropertyName("context_description")]
        public string ContextDescription { get; set; }
    }
}
