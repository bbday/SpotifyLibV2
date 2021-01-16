using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Refit;
using SpotifyLibV2.Attributes;
using SpotifyLibV2.Models.Request;
using SpotifyLibV2.Models.Response;

namespace SpotifyLibV2.Api
{
    [BaseUrl("https://api.spotify.com")]
    public interface ILibrary
    {
        [Get("/v1/me/tracks/contains")]
        Task<List<bool>> CheckTracks(LibraryCheckTracksRequest request);
    }
}
