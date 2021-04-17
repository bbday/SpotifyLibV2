using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Refit;
using SpotifyLibrary.Attributes;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Clients
{
    [OpenUrlEndpoint]
    public interface IUsersClient
    {
        [Get("/v1/users/{user_id}")]
        Task<PublicUser> GetUser(string user_id);
    }
}