using Newtonsoft.Json;

namespace Spotify.Lib.Models.Response
{
    public struct AcknowledgedResponse
    {
        [JsonProperty("ack_id")] public string AckId { get; set; }
    }
}