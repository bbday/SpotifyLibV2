using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SpotifyLibV2.Mercury;
using SpotifyLibV2.Models.Public;

namespace SpotifyLibV2.Api
{
    public class TokenProvider : ITokensProvider
    {
        private static readonly int TOKEN_EXPIRE_THRESHOLD = 10;
        private readonly IMercuryClient _mercuryClient;
        private readonly List<StoredToken> tokens = new List<StoredToken>();

        public TokenProvider(IMercuryClient mercuryClient)
        {
            _mercuryClient = mercuryClient;
        }

        private StoredToken FindTokenWithAllScopes(string[] scopes)
        {
            return tokens.FirstOrDefault(token => token.HasScopes(scopes));
        }

        public StoredToken GetToken(params string[] scopes)
        {
            if (scopes.Length == 0) throw new ArgumentOutOfRangeException(
                nameof(scopes),
                "provide atleast 1 scope");

            var token = FindTokenWithAllScopes(scopes);
            if (token != null)
            {
                if (token.Expired()) tokens.Remove(token);
                else return token;
            }

            Debug.WriteLine(
                $"Token expired or not suitable, requesting again. scopes: {string.Join(",", scopes)}, oldToken: {token}");

            token = _mercuryClient.SendSync(MercuryRequests.RequestToken("", scopes));

            Debug.WriteLine($"Updated token successfully! scopes: {string.Join(",", scopes)}, newToken: {token}");
            tokens.Add(token);
            return token;
        }
    }
}
