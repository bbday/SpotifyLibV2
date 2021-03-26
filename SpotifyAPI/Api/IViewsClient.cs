using System.Threading.Tasks;
using Refit;
using SpotifyLibrary.Attributes;
using SpotifyLibrary.Models.Request;
using SpotifyLibrary.Models.Response.Interfaces;
using SpotifyLibrary.Models.Response.Paging;
using SpotifyLibrary.Models.Response.Views;

namespace SpotifyLibrary.Api
{
    [OpenUrlEndpoint]
    public interface IViewsClient
    {
        [Get("/v1/views/personalized-recommendations")]
        Task<ViewWrapper<ViewWrapper<ISpotifyItem>>> GetHomeView(HomeRequest request);
    }
}
