using System;
using System.Collections.Generic;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpotifyLibrary.Enums;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class FullEpisode : SimpleEpisode, ITrackItem
    {
        private List<IAudioItem> _artists;

        [JsonProperty("html_description")]
        public string HtmlDescription { get; set; }
        public override AudioItemType Type => AudioItemType.Episode;
        [JsonProperty("show")]
        public SimpleShow Show { get; set; }
        [JsonProperty("duration_ms")]
        public int DurationMs { get; set; }
        [JsonProperty("is_playable")]
        public bool CanPlay { get; set; }

        public TrackType TrackType => TrackType.Podcast;
        public TimeSpan DurationTs => TimeSpan.FromMilliseconds(DurationMs);
        public IAudioItem? Group => Show;
        public long? Playcount { get; }

        public List<IAudioItem> Artists => _artists ??= new List<IAudioItem>(1)
        {
            Show
        };

        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseDatePrecision ReleaseDatePrecision { get; set; }
        [JsonProperty("resume_point")]
        public ResumePoint ResumePoint { get; set; }
        public string Language { get; set; }
        public List<string> Languages { get; set; }
        public override string Description { get; set; }
    }
}
