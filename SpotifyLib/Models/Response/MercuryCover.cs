using System.Text.Json.Serialization;

namespace SpotifyLib.Models.Response
{
    public readonly struct MercuryCover
    {
        [JsonConstructor]
        public MercuryCover(string? uri, string? url)
        {
            Uri = uri;
            Url = url;
        }

        /// <summary>
        /// https CDN Url.
        /// </summary>
        [JsonPropertyName("uri")]
        public string Uri { get; }
        [JsonPropertyName("url")]
        public string Url { get; }
    }
}
