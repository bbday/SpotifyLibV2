using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpotifyLibrary.Models.Response.Pathfinder
{
    public class PathfinderColors
    {
        [JsonProperty("extractedColor")]
        public ExtractedColor ExtractedColor { get; set; }
    }
    public class PathfinderColorsAsArray
    {
        [JsonProperty("extractedColors")]
        public List<ExtractedColor> ExtractedColor { get; set; }
    }

    public class ExtractedColor
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

    public class Color
    {
        [JsonProperty("hex")]
        public string Hex { get; set; }

        [JsonProperty("isFallback")]
        public bool IsFallback { get; set; }
    }
}