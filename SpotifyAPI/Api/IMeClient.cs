using System.Threading.Tasks;
using Refit;
using SpotifyLibrary.Attributes;
using SpotifyLibrary.Models.Response;

namespace SpotifyLibrary.Api
{
    [OpenUrlEndpoint]
    public interface IMeClient
    {
        [Get("/v1/me")]
        Task<PrivateUser> GetCurrentUser();
    }
}