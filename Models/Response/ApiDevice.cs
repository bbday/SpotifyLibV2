using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SpotifyLibV2.Models.Response
{
    public class DeviceResponse
    {
        public List<ApiDevice> Devices { get; set; } = default!;
    }
    public partial class ApiDevice
    {
        public string Id { get; set; } = default!;
        public bool IsActive { get; set; }
        public bool IsPrivateSession { get; set; }
        public bool IsRestricted { get; set; }
        public string Name { get; set; } = default!;
        public string Type { get; set; } = default!;
        [JsonProperty("volume_percent")]
        public int? VolumePercent { get; set; }
    }
}
