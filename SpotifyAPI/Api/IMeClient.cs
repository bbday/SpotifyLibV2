using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Refit;
using SpotifyLibrary.Attributes;
using SpotifyLibrary.Models.Request;
using SpotifyLibrary.Models.Response;
using SpotifyLibrary.Models.Response.Interfaces;
using SpotifyLibrary.Models.Response.Views;

namespace SpotifyLibrary.Api
{
    [OpenUrlEndpoint]
    public interface IMeClient
    {
        [Get("/v1/me")]
        Task<PrivateUser> GetCurrentUser();
    }
}