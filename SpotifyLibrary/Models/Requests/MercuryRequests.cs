using SpotifyLibrary.Models.Response;
using SpotifyLibrary.Models.Response.Mercury;

namespace SpotifyLibrary.Models.Requests
{
    public static class MercuryRequests
    {
        private static readonly string KEYMASTER_CLIENT_ID = "65b708073fc0480ea92a077233ca87bd";

        public static SystemTextJsonMercuryRequest<StoredToken> RequestToken(string deviceId, 
            string[] scope)
        {
            return new(RawMercuryRequest.Get(
                $"hm://keymaster/token/authenticated?scope={string.Join(",", scope)}&client_id={KEYMASTER_CLIENT_ID}&device_id={deviceId}"));
        }

        public static JsonMercuryRequest<MercuryContextWrapperResponse> ResolveContext(string uri)
        {
            return new(RawMercuryRequest.Get(
                $"hm://context-resolve/v1/{uri}"));
        }

        public static JsonMercuryRequest<MercuryHub> GetHub(string country, string locale)
        {
            return new JsonMercuryRequest<MercuryHub>(
                RawMercuryRequest.Get($"hm://hubview/km/v2/browse/?format=json&country={country}&locale={locale}"));
        }
    }
}
