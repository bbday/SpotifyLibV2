using System.Text.Json.Serialization;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models
{
    public readonly struct Quick
    {
        [JsonConstructor]
        public Quick(SpotifyId uri, string name)
        {
            Uri = uri;
            Name = name;
        }

        [JsonConverter(typeof(UriToSpotifyIdConverter))]
        public SpotifyId Uri { get; }
        public string Name { get; }
    }
}
