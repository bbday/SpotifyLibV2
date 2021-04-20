using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Refit;
using SpotifyLibrary.Attributes;
using SpotifyLibrary.Models.Requests;
using SpotifyLibrary.Models.Response;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Clients
{
    [OpenUrlEndpoint]
    public interface IEpisodesClient
    {
        [Get("/v1/episodes/{id}")]
        Task<FullEpisode> GetEpisode(string id);
        [Get("/v1/episodes")]
        Task<EpisodesResponse> GetEpisodes(TracksRequest ids);
    }
}