using System.Threading.Tasks;
using Refit;
using SpotifyLibrary.Attributes;
using SpotifyLibrary.Models.Request;
using SpotifyLibrary.Models.Response;

namespace SpotifyLibrary.Api
{
    [OpenUrlEndpoint]
    public interface IAlbumsClient
    {
        /// <summary>
        /// Get Spotify catalog information for multiple albums identified by their Spotify IDs.
        /// </summary>
        /// <param name="request">The request-model which contains required and optional parameters</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-get-multiple-albums
        /// </remarks>
        /// <returns></returns>
        [Get("/v1/albums")]
        Task<AlbumsResponse> GetSeveral(AlbumsRequest request);
    }
}
