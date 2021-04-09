using System;
using System.Collections.Generic;
using MusicLibrary.Enum;
using MusicLibrary.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Response.Mercury;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class FullEpisode : SimpleEpisode
    {
        [JsonProperty("html_description")]
        public string HtmlDescription { get; set; }
        public override AudioType Type => AudioType.Episode;
        [JsonProperty("show")]
        public SimpleShow Show { get; set; }
        [JsonProperty("duration_ms")]
        public int DurationMs { get; set; }
        [JsonProperty("is_playable")]
        public bool CanPlay { get; set; }
        public TimeSpan? DurationTs => TimeSpan.FromMilliseconds(DurationMs);
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
