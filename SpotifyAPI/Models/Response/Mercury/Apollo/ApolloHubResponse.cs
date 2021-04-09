using System.Collections.Generic;
using Newtonsoft.Json;
using SpotifyLibrary.Helpers.JsonConverters;

namespace SpotifyLibrary.Models.Response.Mercury.Apollo
{
    public class ApolloHubResponse
    {
        public string Title { get; set; }
        [JsonConverter(typeof(ApolloConverter))]
        public List<IApolloHubItem> Body { get; set; }
    }
}
