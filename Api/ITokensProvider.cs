using SpotifyLibV2.Models.Public;

namespace SpotifyLibV2.Api
{
    public interface ITokensProvider
    {
        StoredToken GetToken(params string[] scopes);
    }
}
