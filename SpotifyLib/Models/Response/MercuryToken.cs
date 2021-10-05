using System;
using System.Text.Json.Serialization;

namespace SpotifyLib.Models.Response
{
    public static class TimeHelper
    {
        internal static readonly DateTime Jan1St1970 = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long CurrentTimeMillisSystem => (long)(DateTime.UtcNow - Jan1St1970).TotalMilliseconds;
    }
    public readonly struct MercuryToken
    {
        [JsonConstructor]
        public MercuryToken(string accessToken, int expiresIn, string tokenType, string[] scope)
        {
            AccessToken = accessToken;
            ExpiresIn = expiresIn;
            TokenType = tokenType;
            Scope = scope;
            CreatedAt = TimeHelper.CurrentTimeMillisSystem;
        }

        [JsonPropertyName("accessToken")] public string AccessToken { get; }
        [JsonPropertyName("expiresIn")] public int ExpiresIn { get; }

        [JsonPropertyName("tokenType")] public string TokenType { get; }

        [JsonPropertyName("scope")] public string[] Scope { get; }

        [JsonIgnore] public long CreatedAt { get; }
    }

    public static class MercuryTokenExtensions
    {
        private const int TokenExpireThreshold = 10;

        public static bool IsExpired(this MercuryToken token) =>
            token.CreatedAt + (token.ExpiresIn - TokenExpireThreshold) * 1000 < TimeHelper.CurrentTimeMillisSystem;
    }
}
