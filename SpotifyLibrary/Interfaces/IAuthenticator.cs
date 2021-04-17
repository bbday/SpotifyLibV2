using System.Threading.Tasks;
using Spotify;

namespace SpotifyLibrary.Interfaces
{
    public interface IAuthenticator
    {
        Task<LoginCredentials> Get();
    }
}
