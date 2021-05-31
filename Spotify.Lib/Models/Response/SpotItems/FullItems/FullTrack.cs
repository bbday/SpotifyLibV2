using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Spotify.Lib.Helpers;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models.Ids;
using Spotify.Lib.Models.TracksnEpisodes;

namespace Spotify.Lib.Models.Response.SpotItems.FullItems
{
    public struct FullTrack : ITrackItem
    {
        public string Uri { get; set; }
        public AudioItemType Type => AudioItemType.Track;
        public ISpotifyId Id => new TrackId(Uri);
        public string Name { get; set; }
        public string Description => string.Join(", ", Artists.Select(z => z.Name));
        public string Caption { get; set; }
        public List<UrlImage> Images => Group.Images;
        [JsonProperty("duration_ms")]
        public int DurationMs { get; set; }
        [JsonProperty("is_explicit")]
        public bool Explicit { get; set; }
        public TrackType TrackType => TrackType.Track;
        public TimeSpan DurationTs => TimeSpan.FromMilliseconds(DurationMs);
        [JsonProperty("album")]
        [JsonConverter(typeof(SpotifyItemConverter))]
        public ISpotifyItem? Group { get; set; }
        public long? Playcount { get; set; }

        [JsonConverter(typeof(SpotifyItemConverter))]
        public List<ISpotifyItem> Artists { get; set; }
        [JsonProperty("is_playable")]
        public bool CanPlay { get; set; }
    }
}
