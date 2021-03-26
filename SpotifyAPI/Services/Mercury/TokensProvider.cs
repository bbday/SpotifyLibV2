using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SpotifyLibrary.Models.Request;
using SpotifyLibrary.Models.Response;
using SpotifyLibrary.Services.Mercury.Interfaces;

namespace SpotifyLibrary.Services.Mercury
{
    public class TokensProvider : ITokensProvider
    {
        private static readonly int TokenExpireThreshold = 10;
        private readonly IMercuryClient _mercuryClient;
        private readonly List<StoredToken> _tokens = new();

        public TokensProvider(IMercuryClient mercuryClient)
        {
            _mercuryClient = mercuryClient;
        }

        public async Task<StoredToken> GetToken(params string[] scopes)
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

            token = await _mercuryClient.SendAsync(MercuryRequests.RequestToken("", scopes));

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