using System.Collections.Generic;
using Newtonsoft.Json;

namespace Spotify.Lib.Models.Response
{
    public class Source
    {
        public string url { get; set; }
    }
    public class HeaderImage
    {
        public List<Source> sources { get; set; }
    }

    public class Visuals
    {
        public HeaderImage headerImage { get; set; }
    }

    public class Artist
    {
        public Visuals visuals { get; set; }
    }

    public class PathfinderFullScreen
    {
        public Artist artist { get; set; }
    }

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