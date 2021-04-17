using SpotifyLibrary.Models.Response;

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
    }
}
