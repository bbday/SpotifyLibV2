using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Refit;
using SpotifyLibrary.Attributes;
using SpotifyLibrary.Models.Response;
using SpotifyLibrary.Models.Response.Interfaces;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Api
{
    [OpenUrlEndpoint]
    public interface ITracksClient
    {
        [Get("/v1/tracks/{id}")]
        Task<FullTrack> GetTrack(string id);
    }
}