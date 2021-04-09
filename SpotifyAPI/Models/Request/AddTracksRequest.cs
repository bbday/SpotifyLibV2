using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpotifyLibrary.Models.Request
{
    public class AddTracksRequest
    {
        public AddTracksRequest(IEnumerable<string> uris,
            int? position = null)
        {
            Uris = uris;
            Position = position;
        }
        [JsonProperty("position")]
        public int? Position { get; }
        [JsonProperty("uris")]
        public IEnumerable<string> Uris { get; }
    }
}
