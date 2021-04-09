using Newtonsoft.Json;

namespace SpotifyLibrary.Models.Response
{
    public class AcknowledgedResponse
    {
        [JsonProperty("ack_id")]
        public string AckId { get; set; }
    }
}
