using System.Threading.Tasks;
using Refit;
using SpotifyLibV2.Attributes;
using SpotifyLibV2.Models.Request;
using SpotifyLibV2.Models.Response;

namespace SpotifyLibV2.Api
{

    [BaseUrl("https://api.spotify.com")]
    public interface IHomeClient
    {
        [Get("/v1/views/desktop-home")]
        Task<HomeResponse> GetHome(HomeRequest request);
    }
}
