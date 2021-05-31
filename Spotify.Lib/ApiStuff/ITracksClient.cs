using System.Threading.Tasks;
using Refit;
using Spotify.Lib.Attributes;
using Spotify.Lib.Models.Request;
using Spotify.Lib.Models.Response;
using Spotify.Lib.Models.Response.SpotItems.FullItems;

namespace Spotify.Lib.ApiStuff
{
    [OpenUrlEndpoint]
    public interface ITracksClient
    {
        [Get("/v1/tracks/{id}")]
        Task<FullTrack> GetTrack(string id);
        [Get("/v1/tracks")]
        Task<TracksResponse> GetSeveral(TracksRequest req);
    }
}