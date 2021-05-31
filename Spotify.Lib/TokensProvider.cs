using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Spotify.Lib.Helpers;
using Spotify.Lib.Models.Response.Mercury;

namespace Spotify.Lib
{
    public class TokensClient
    {
        private static readonly int TokenExpireThreshold = 10;
        private readonly List<StoredToken> _tokens = new();

        public async Task<StoredToken> GetToken(CancellationToken ct, params string[] scopes)
        {
            if (scopes.Length == 0)
                throw new ArgumentOutOfRangeException(
                    nameof(scopes),
                    "provide atleast 1 scope");

            var token = FindTokenWithAllScopes(scopes);
            if (token != null)
            {
                if (token.Expired()) _tokens.Remove(token);
                else return token;
            }

            Debug.WriteLine(
                $"Token expired or not suitable, requesting again. scopes: {string.Join(",", scopes)}, oldToken: {token}");

            token = await SpotifyClient.Instance.SendAsyncReturnJson<StoredToken>(
                MercuryRequests.RequestToken("", scopes), ct);

            Debug.WriteLine($"Updated token successfully! scopes: {string.Join(",", scopes)}, newToken: {token}");
            _tokens.Add(token);
            return token;
        }

        private StoredToken FindTokenWithAllScopes(string[] scopes)
        {
            return _tokens.FirstOrDefault(token => token.HasScopes(scopes));
        }
    }
}