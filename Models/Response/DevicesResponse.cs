using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SpotifyLibV2.Models.Response
{
    public class DevicesResponse
    {
        [JsonProperty("devices")]
        [JsonPropertyName("devices")]
        public List<Device> Devices { get; set; } = default!;
    }
    public class Device
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;
        [JsonProperty("is_active")]
        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        [JsonProperty("is_private_session")]
        [JsonPropertyName("is_private_session")]
        public bool IsPrivateSession { get; set; }

        [JsonProperty("is_restricted")]
        [JsonPropertyName("is_restricted")]
        public bool IsRestricted { get; set; }

        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        [JsonPropertyName("type")]
        [JsonProperty("type")]
        public string Type { get; set; } = default!;

        [JsonPropertyName("volume_percent")]
        [JsonProperty("volume_percent")]
        public int? VolumePercent { get; set; }
    }
}
