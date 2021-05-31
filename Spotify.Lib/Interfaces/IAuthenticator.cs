using System.Threading.Tasks;

namespace Spotify.Lib.Interfaces
{
    public interface IAuthenticator
    {
        Task<LoginCredentials> Get();
    }
}