using System.Threading.Tasks;
using Spotify;

namespace SpotifyLibrary.Authentication
{
    public interface IAuthenticator
    {
        Task<LoginCredentials> Get();
    }
}