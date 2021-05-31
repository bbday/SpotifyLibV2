using System.Threading.Tasks;
using Refit;
using Spotify.Lib.Attributes;
using Spotify.Lib.Models.Response.SpotItems.SimpleItems;
using Spotify.Lib.Models.Response.SpotItems.Views;

namespace Spotify.Lib.ApiStuff
{
    [OpenUrlEndpoint]
    public interface IUsersClient
    {
        [Get("/v1/users/{user_id}")]
        Task<PublicUser> GetUser(string user_id);

    }
}