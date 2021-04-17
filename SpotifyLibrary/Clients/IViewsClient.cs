using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MediaLibrary.Interfaces;
using Refit;
using SpotifyLibrary.Attributes;
using SpotifyLibrary.Models.Requests;
using SpotifyLibrary.Models.Response.Views;

namespace SpotifyLibrary.Clients
{
    [OpenUrlEndpoint]
    public interface IViewsClient
    {
        [Get("/v1/views/desktop-home")]
        Task<ViewWrapper<ViewWrapper<IAudioItem>>> GetHomeView(HomeRequest request);
        [Get("/v1/views/{id}")]
        Task<ViewWrapper<IAudioItem>> GetCustomView(string id, HomeRequest request);
        [Get("/v1/views/{id}")]
        Task<ViewWrapper<T>> GetCustomViewForCustomType<T>(string id, HomeRequest request);
    }
}
