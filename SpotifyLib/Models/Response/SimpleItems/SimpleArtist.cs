using System.Collections.Generic;
using System.Text.Json.Serialization;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models.Response.SimpleItems
{
    public readonly struct SimpleArtist : ISpotifyItem
    {
        [JsonConstructor]
        public SimpleArtist(string name, List<UrlImage> images,
            SpotifyId uri)
        {
            Name = name;
            Images = images;
            Uri = uri;
        }

        public string Name { get; }
        public List<UrlImage> Images { get;  }
        [JsonConverter(typeof(UriToSpotifyIdConverter))]
        public SpotifyId Uri { get; }
    }
}
