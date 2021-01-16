using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SpotifyLibV2.Models.Shared
{
    public class SpotifyCover
    {
        [JsonProperty("uri")] public string Uri { get; set; }
    }
}