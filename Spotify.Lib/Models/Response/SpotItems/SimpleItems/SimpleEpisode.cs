using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models.Ids;

namespace Spotify.Lib.Models.Response.SpotItems.SimpleItems
{
    public struct SimpleEpisode : ISpotifyItem
    {
        public string Uri { get; set; }
        public AudioItemType Type => AudioItemType.Episode;
        public ISpotifyId Id => new EpisodeId(Uri);
        public string Name { get; set; }
        public string Description { get; set; }

        public string Caption { get; set; }
        public List<UrlImage> Images { get; set; }
        [JsonProperty("release_date")] public string ReleaseDate { get; set; }
        [JsonProperty("duration_ms")] public double DurationMs { get; set; }
        [JsonProperty("is_explicit")] public bool Explicit { get; set; }
    }
}