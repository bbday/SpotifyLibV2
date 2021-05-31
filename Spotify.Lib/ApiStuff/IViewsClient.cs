using System.Threading.Tasks;
using Refit;
using Spotify.Lib.ApiStuff.Models.Request;
using Spotify.Lib.Attributes;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models.Response.SpotItems.Views;

namespace Spotify.Lib.ApiStuff
{
    [OpenUrlEndpoint]
    public interface IViewsClient
    {
        [Get("/v1/views/desktop-home")]
        Task<ViewWrapper<ViewWrapper<ISpotifyItem>>> GetHomeView(HomeRequest request);

        [Get("/v1/views/{id}")]
        Task<ViewWrapper<ISpotifyItem>> GetCustomView(string id, HomeRequest request);

        [Get("/v1/views/{id}")]
        Task<ViewWrapper<T>> GetCustomViewForCustomType<T>(string id, HomeRequest request);
    }
}