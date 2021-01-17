using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SpotifyLibV2.Models.Response
{
    public partial class PathfinderColors
    {
        [JsonProperty("extractedColor")]
        public ExtractedColor ExtractedColor { get; set; }
    }

    public partial class ExtractedColor
    {
        [JsonProperty("__typename")]
        public string Typename { get; set; }

        [JsonProperty("colorRaw")]
        public Color ColorRaw { get; set; }

        [JsonProperty("colorDark")]
        public Color ColorDark { get; set; }

        [JsonProperty("colorLight")]
        public Color ColorLight { get; set; }
    }

    public partial class Color
    {
        [JsonProperty("hex")]
        public string Hex { get; set; }

        [JsonProperty("isFallback")]
        public bool IsFallback { get; set; }
    }
}