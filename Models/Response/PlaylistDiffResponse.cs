using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Spotify.Playlist4.Proto;

namespace SpotifyLibV2.Models.Response
{
    public class PlaylistDiffResponse
    {
        [JsonPropertyName("revision")]
        [JsonProperty("revision")]
        public string Revision { get; set; }
        [JsonPropertyName("diff")]
        [JsonProperty("diff")]
        public Diff Diff { get; set; }
        [JsonPropertyName("abuseReportingEnabled")]
        [JsonProperty("abuseReportingEnabled")]
        public bool abuseReportingEnabled { get; set; }
    }
}
