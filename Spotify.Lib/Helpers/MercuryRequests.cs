using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spotify.Lib.Helpers;
using Spotify.Lib.Models;

namespace Spotify.Lib.Helpers
{
    public class MercuryContextWrapperResponse
    {
        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("pages")]
        public JArray Pages { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class Metadata
    {
        [JsonProperty("context_description")]
        public string ContextDescription { get; set; }
    }
}

public static class MercuryRequests
{
    private static readonly string KEYMASTER_CLIENT_ID = "65b708073fc0480ea92a077233ca87bd";

    public static RawMercuryRequest RequestToken(string deviceId,
        string[] scope)
    {
        return RawMercuryRequest.Get(
            $"hm://keymaster/token/authenticated?scope={string.Join(",", scope)}&client_id={KEYMASTER_CLIENT_ID}&device_id={deviceId}");
    }

    public static SystemTextJsonMercuryRequest<MercuryContextWrapperResponse> ResolveContext(string uri)
    {
        return new(RawMercuryRequest.Get(
            $"hm://context-resolve/v1/{uri}"));
    }

    //public static JsonMercuryRequest<MercuryHub> GetHub(string country, string locale)
    //{
    //    return new JsonMercuryRequest<MercuryHub>(
    //        RawMercuryRequest.Get($"hm://hubview/km/v2/browse/?format=json&country={country}&locale={locale}"));
    //}
}