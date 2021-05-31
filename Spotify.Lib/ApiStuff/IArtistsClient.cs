using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Refit;
using Spotify.Lib.Attributes;
using Spotify.Lib.Models.Request;
using Spotify.Lib.Models.Response;

namespace SpotifyLibrary.Clients
{
    [BaseUrl("https://api.spotify.com")]
    public interface IArtistsClient
    {
        [Get("/v1/artists")]
        Task<ArtistsResponse> GetSeveral(TracksRequest request);
    }
}