using System.Threading.Tasks;
using Spotify;

namespace SpotifyLibV2.Authentication
{
    public interface IAuthenticator
    {
        Task<LoginCredentials> Get();
    }
}


