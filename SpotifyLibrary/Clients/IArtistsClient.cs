using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Refit;
using SpotifyLibrary.Attributes;
using SpotifyLibrary.Models.Requests;
using SpotifyLibrary.Models.Response;

namespace SpotifyLibrary.Clients
{
    [BaseUrl("https://api.spotify.com")]
    public interface IArtistsClient
    {
        [Get("/v1/artists")]
        Task<ArtistsResponse> GetSeveral(ArtistsRequest request);
    }
}