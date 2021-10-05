using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models.Response.SearchItems
{
    public readonly struct SearchTrack : ISpotifyItem
    {
        [JsonConstructor]
        public SearchTrack(SpotifyId uri,
            string name,
            string image,
            TimeSpan duration,
            IEnumerable<Quick> artists,
            Quick album)
        {
            Name = name;
            Uri = uri;
            Image = image;
            Artists = artists;
            Album = album;
            Duration = duration;
        }
        [JsonConverter(typeof(UriToSpotifyIdConverter))]
        public SpotifyId Uri { get; }
        public string Name { get; }
        public string Image { get; }
        public IEnumerable<Quick> Artists { get; }
        public Quick Album { get; }
        [JsonConverter(typeof(DoubleToTimeSpanConverter))]
        public TimeSpan Duration { get; }
    }
}
