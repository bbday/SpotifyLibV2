using System.Threading.Tasks;
using Refit;
using Spotify.Lib.Attributes;
using Spotify.Lib.Models.Response.SpotItems.FullItems;

namespace Spotify.Lib.ApiStuff
{
    [OpenUrlEndpoint]
    public interface IEpisodesClient
    {
        [Get("/v1/episodes/{id}")]
        Task<FullEpisode> GetEpisode(string id);
    }
}