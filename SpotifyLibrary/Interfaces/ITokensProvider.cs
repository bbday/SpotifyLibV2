using System.Threading.Tasks;
using SpotifyLibrary.Models.Response;

namespace SpotifyLibrary.Interfaces
{
    public interface ITokensProvider
    {
        Task<StoredToken> GetToken(params string[] scopes);
    }
}