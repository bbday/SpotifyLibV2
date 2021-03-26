using System.Threading.Tasks;
using SpotifyLibrary.Models.Response;

namespace SpotifyLibrary.Services.Mercury.Interfaces
{
    public interface ITokensProvider
    {
        Task<StoredToken> GetToken(params string[] scopes);
    }
}