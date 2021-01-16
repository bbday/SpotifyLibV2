using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SpotifyLibV2.Models.Response
{
    public class ApiContext
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }
    }
}
