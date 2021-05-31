using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Spotify.Lib.Helpers;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models.Ids;
using Spotify.Lib.Models.TracksnEpisodes;

namespace Spotify.Lib.Models.Response.SpotItems.FullItems
{
    public struct FullEpisode : ITrackItem
    {
        public string Uri { get; set; }
        public AudioItemType Type => AudioItemType.Episode;
        public ISpotifyId Id => new EpisodeId(Uri);
        public string Name { get; set; }
        public string Description { get; set; }

        public string Caption { get; set; }
        public List<UrlImage> Images { get; set; }
        [JsonProperty("release_date")] public string ReleaseDate { get; set; }
        [JsonProperty("duration_ms")] public int DurationMs { get; set; }
        [JsonProperty("is_explicit")] public bool Explicit { get; set; }
        public TrackType TrackType => TrackType.Episode;
        public TimeSpan DurationTs => TimeSpan.FromMilliseconds(DurationMs);

        [JsonProperty("show")]
        [JsonConverter(typeof(SpotifyItemConverter))]
        public ISpotifyItem? Group { get; set; }

        public long? Playcount { get; }

        public List<ISpotifyItem> Artists => new(1)
        {
            Group
        };
    }
}