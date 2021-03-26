using JetBrains.Annotations;
using SpotifyLibrary.Models.Response;

namespace SpotifyLibrary.Models.Request
{
    public static class MercuryRequests
    {
        private static readonly string KEYMASTER_CLIENT_ID = "65b708073fc0480ea92a077233ca87bd";
        public static JsonMercuryRequest<MercuryContextWrapperResponse> ResolveContext([NotNull] string uri)
        {
            return new(RawMercuryRequest.Get(
                $"hm://context-resolve/v1/{uri}"));
        }
        public static JsonMercuryRequest<StoredToken> RequestToken([NotNull] string deviceId, [NotNull] string[] scope)
        {
            return new(RawMercuryRequest.Get(
                $"hm://keymaster/token/authenticated?scope={string.Join(",", scope)}&client_id={KEYMASTER_CLIENT_ID}&device_id={deviceId}"));
        }
    }
}