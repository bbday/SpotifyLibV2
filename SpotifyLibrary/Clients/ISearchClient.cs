using System.Threading.Tasks;
using Refit;
using SpotifyLibrary.Attributes;
using SpotifyLibrary.Models.Requests;
using SpotifyLibrary.Models.Response;

namespace SpotifyLibrary.Api
{

    [OpenUrlEndpoint]
    public interface ISearchClient
    {
        [Get("/v1/search")]
        Task<ApiSearchResponse> Search(ApiSearchRequest request);
    }
}