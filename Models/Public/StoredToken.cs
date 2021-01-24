using System.Linq;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SpotifyLibV2.Api;

namespace SpotifyLibV2.Models.Public
{
    public class StoredToken
    {
        [JsonPropertyName("accessToken")] public string AccessToken { get; set; }
        [JsonPropertyName("expiresIn")] public int ExpiresIn { get; set; }
        [JsonPropertyName("tokenType")] public string TokenType { get; set; }
        [JsonPropertyName("scope")] public string[] Scope { get; set; }

        private readonly long _timeStamp;
        private const int TOKEN_EXPIRE_THRESHOLD = 10;

        public StoredToken()
        {
            _timeStamp = TimeProvider.CurrentTimeMillis();
        }

        public bool Expired() =>
            _timeStamp + (ExpiresIn - TOKEN_EXPIRE_THRESHOLD) * 1000 < TimeProvider.CurrentTimeMillis();

        public override string ToString()
        {
            return "StoredToken{" +
                   "expiresIn=" + ExpiresIn +
                   ", accessToken='" + AccessToken +
                   "', scopes=" + string.Join(",", Scope) +
                   ", timestamp=" + _timeStamp +
                   '}';
        }

        public bool HasScope([NotNull] string scope) => Scope.ToList().Exists(x => x == scope);

        public bool HasScopes(string[] sc) =>
            Scope.OrderBy(kvp => kvp)
                .SequenceEqual(sc.OrderBy(kvp => kvp));
    }
}