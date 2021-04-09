using System.Threading.Tasks;
using Refit;
using SpotifyLibrary.Attributes;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Api
{
    [OpenUrlEndpoint]
    public interface IEpisodes
    {
        [Get("/v1/episodes/{id}")]
        Task<FullEpisode> GetEpisode(string id);
    }
}