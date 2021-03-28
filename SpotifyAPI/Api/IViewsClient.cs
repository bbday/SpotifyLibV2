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
        [Get("/v1/views/desktop-home")]
        Task<ViewWrapper<ViewWrapper<IAudioItem>>> GetHomeView(HomeRequest request);
        [Get("/v1/views/{id}")]
        Task<ViewWrapper<IAudioItem>> GetCustomView(string id, HomeRequest request);
    }
}
