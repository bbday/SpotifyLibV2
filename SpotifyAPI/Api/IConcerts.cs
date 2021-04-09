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
    [ResolvedSpClientEndpoint]
    public interface IConcerts
    {
        [Get("/concerts/v1/location/suggest")]
        Task<LocationsResponse> SuggestLocation([AliasAs("q")]string query);

        [Get("/concerts/v2/concerts/artist/{id}")]
        Task<ConcertsResponse> GetConcerts(string id, ConcertsRequest request);
    }
}
