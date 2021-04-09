using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Refit;
using SpotifyLibrary.Attributes;
using SpotifyLibrary.Models.Request;
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