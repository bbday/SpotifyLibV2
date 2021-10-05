using System.Collections.Generic;
using System.Text.Json.Serialization;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models.Response.SimpleItems
{
    public readonly struct SimpleShow : ISpotifyItem
    {
        [JsonConstructor]
        public SimpleShow(string name, List<UrlImage> images, 
            SpotifyId uri,  bool @explicit, string publisher, List<Copyright> copyrights, string description)
        {
            Name = name;
            Images = images;
            Uri = uri;
            Explicit = @explicit;
            Publisher = publisher;
            Copyrights = copyrights;
            Description = description;
        }

        [JsonConverter(typeof(UriToSpotifyIdConverter))]
        public SpotifyId Uri { get; }
        public string Name { get; }
        public List<UrlImage> Images { get; }
        public string Publisher { get;  }
        public List<Copyright> Copyrights { get; }
        [JsonPropertyName("is_explicit")] 
        public bool Explicit { get;  }

        public string Description { get; }
    }

    public struct Copyright
    {
        public string Text { get; set; }
        public string Type { get; set; }
    }
}
