using System.Linq;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using SpotifyLibrary.Helpers;

namespace SpotifyLibrary.Models.Response
{
    public class StoredToken
    {
        private const int TOKEN_EXPIRE_THRESHOLD = 10;

        private readonly long _timeStamp;

        public StoredToken()
        {
            _timeStamp = TimeProvider.CurrentTimeMillis();
        }

        [JsonProperty("accessToken")]
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }
        [JsonProperty("expiresIn")]
        [JsonPropertyName("expiresIn")]
        public int ExpiresIn { get; set; }
        [JsonProperty("tokenType")]
        [JsonPropertyName("tokenType")]
        public string TokenType { get; set; }
        [JsonProperty("scope")]
        [JsonPropertyName("scope")]
        public string[] Scope { get; set; }

        public bool Expired()
        {
            return _timeStamp + (ExpiresIn - TOKEN_EXPIRE_THRESHOLD) * 1000 < TimeProvider.CurrentTimeMillis();
        }

        public override string ToString()
        {
            return "StoredToken{" +
                   "expiresIn=" + ExpiresIn +
                   ", accessToken='" + AccessToken +
                   "', scopes=" + string.Join(",", Scope) +
                   ", timestamp=" + _timeStamp +
                   '}';
        }

        public bool HasScope(string scope)
        {
            return Scope.ToList().Exists(x => x == scope);
        }

        public bool HasScopes(string[] sc)
        {
            return Scope.OrderBy(kvp => kvp)
                .SequenceEqual(sc.OrderBy(kvp => kvp));
        }
    }
}