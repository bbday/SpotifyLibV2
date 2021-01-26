using System.Threading.Tasks;
using Refit;
using SpotifyLib.Models.Api.Paging;
using SpotifyLibV2.Attributes;
using SpotifyLibV2.Models.Request;
using SpotifyLibV2.Models.Response;

namespace SpotifyLibV2.Api
{
    [BaseUrl("https://api.spotify.com")]
    public interface IMeClient
    {
        /// <summary>
        /// Get the current user’s top tracks based on calculated affinity.
        /// </summary>
        /// <param name="request">The request-model which contains required and optional parameters.</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-get-users-top-artists-and-tracks
        /// </remarks>
        /// <returns></returns>
        [Get("/v1/me/top/tracks")]
        Task<Paging<FullTrack>> GetTopTracks(PersonalizationTopRequest request);


        /// <summary>
        /// Get the current user’s top artists based on calculated affinity.
        /// </summary>
        /// <param name="request">The request-model which contains required and optional parameters.</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-get-users-top-artists-and-tracks
        /// </remarks>
        /// <returns></returns>
        [Get("/v1/me/top/artists")]
        Task<Paging<FullArtist>> GetTopArtists(PersonalizationTopRequest request);
    }
}
