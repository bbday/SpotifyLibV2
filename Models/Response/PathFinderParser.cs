using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace SpotifyLibV2.Models.Response
{
    public class PathFinderParser<T>
    {
        [JsonProperty("errors", NullValueHandling = NullValueHandling.Ignore)]
        public List<Error> Errors { get; set; }
        [JsonProperty("data")] public T Data { get; set; }

        [JsonIgnore]
        public bool HasError => Errors != null && Errors.Any();
    }
    public class Error
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        public string[] Path { get; set; }

    }
}
