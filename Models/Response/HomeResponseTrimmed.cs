using Newtonsoft.Json;

namespace SpotifyLibV2.Models.Response
{
    public class HomeResponseTrimmed
    {
        [JsonProperty("tag_line")] public string TagLine { get; set; }
    }
}