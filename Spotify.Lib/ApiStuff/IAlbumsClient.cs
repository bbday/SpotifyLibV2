using System.Threading.Tasks;
using Refit;
using Spotify.Lib.Attributes;
using Spotify.Lib.Models.Request;
using Spotify.Lib.Models.Response;

namespace Spotify.Lib.ApiStuff
{

    [BaseUrl("https://api.spotify.com")]
    public interface IAlbumsClient
    {
        [Get("/v1/albums")]
        Task<AlbumsResponse> GetSeveral(TracksRequest request);
    }
}