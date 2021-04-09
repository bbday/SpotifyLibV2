using System.Threading.Tasks;
using Refit;
using SpotifyLibrary.Attributes;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Api
{
    [OpenUrlEndpoint]
    public interface IUsersClient
    {
        [Get("/v1/users/{user_id}")]
        Task<PublicUser> GetUser(string user_id);
    }
}