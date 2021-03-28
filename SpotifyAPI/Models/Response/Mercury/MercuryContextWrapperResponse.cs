using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SpotifyLibrary.Models.Response.Mercury
{
    public class MercuryContextWrapperResponse
    {
        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("pages")]
        public JArray Pages { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class Metadata
    {
        [JsonProperty("context_description")]
        public string ContextDescription { get; set; }
    }
}
