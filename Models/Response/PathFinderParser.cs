using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SpotifyLibV2.Models.Response
{
    public class PathFinderParser<T>
    {
        [JsonProperty("errors", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("data")]
        public List<Error> Errors { get; set; }
        [JsonProperty("data")]
        [JsonPropertyName("data")]
        public T Data { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public bool HasError => Errors != null && Errors.Any();
    }
    public class Error
    {
        [JsonProperty("message")]
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("path")]
        public string[] Path { get; set; }

    }
}
