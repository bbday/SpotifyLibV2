using System.Collections.Generic;
using System.Text.Json.Serialization;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models.Response.SimpleItems
{
    public readonly struct PublicUser
    {
        [JsonConstructor]
        public PublicUser(SpotifyId uri, string name, List<UrlImage> images)
        {
            Uri = uri;
            Name = name;
            Images = images;
        }

        [JsonConverter(typeof(UriToSpotifyIdConverter))]
        public SpotifyId Uri { get; }
        [JsonPropertyName("display_name")]
        public string Name { get; }
        public List<UrlImage> Images { get;  }
    }
}
