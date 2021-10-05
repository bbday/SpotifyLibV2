using System.Collections.Generic;
using System.Text.Json.Serialization;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models.Response.SimpleItems
{
    public readonly struct SimpleEpisode : ISpotifyItem
    {
        [JsonConstructor]
        public SimpleEpisode(string name, List<UrlImage> images,
            string releaseDate,
            SpotifyId uri, double durationMs, bool @explicit, string description)
        {
            Name = name;
            Images = images;
            ReleaseDate = releaseDate;
            Uri = uri;
            DurationMs = durationMs;
            Explicit = @explicit;
            Description = description;
        }

        [JsonConverter(typeof(UriToSpotifyIdConverter))]
        public SpotifyId Uri { get; }

        public string Name { get; }
        public List<UrlImage> Images { get; }
        [JsonPropertyName("release_date")] public string ReleaseDate { get; }
        [JsonPropertyName("duration_ms")] public double DurationMs { get; }
        [JsonPropertyName("is_explicit")] public bool Explicit { get; }
        public string Description { get;  }

    }
}
