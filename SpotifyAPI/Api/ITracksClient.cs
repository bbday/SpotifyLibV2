using System.Threading.Tasks;
using Refit;
using SpotifyLibrary.Attributes;
using SpotifyLibrary.Models.Request;
using SpotifyLibrary.Models.Response;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Api
{
    [OpenUrlEndpoint]
    public interface ITracksClient
    {
        [Get("/v1/tracks/{id}")]
        Task<FullTrack> GetTrack(string id);

        /// <summary>
        /// Get Spotify catalog information for a single track identified by its unique Spotify ID.
        /// </summary>
        /// <param name="trackId">The Spotify ID for the track.</param>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        [Get("/v1/tracks")]
        Task<TracksResponse> GetSeveral(TracksRequest request);
    }
}