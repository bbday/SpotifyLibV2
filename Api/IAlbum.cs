using System.Threading.Tasks;
using Refit;
using SpotifyLib.Models.Api.Requests;
using SpotifyLibV2.Attributes;
using SpotifyLibV2.Models.Response;

namespace SpotifyLib.Api
{
    [BaseUrl("https://api.spotify.com")]
    public interface IAlbum
    {
        /// <summary>
        /// Get Spotify catalog information for a single album.
        /// </summary>
        /// <param name="albumId">The Spotify ID of the album.</param>
        /// <param name="request">The request-model which contains required and optional parameters</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-get-an-album
        /// </remarks>
        /// <returns></returns>
        [Get("/v1/albums/{albumId}")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716")]
        Task<FullAlbum> Get(string albumId, [AliasAs("market")]string market = "from_token");
    }
}