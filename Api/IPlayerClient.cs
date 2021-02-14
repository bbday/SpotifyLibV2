using System.Threading.Tasks;
using Refit;
using SpotifyLibV2.Attributes;
using SpotifyLibV2.Models.Response;

namespace SpotifyLibV2.Api
{
    [BaseUrl("https://api.spotify.com")]
    public interface IPlayerClient
    {
        /// <summary>
        /// Get information about the user’s current playback state, including track or episode, progress, and active device.
        /// </summary>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-get-information-about-the-users-current-playback
        /// </remarks>
        /// <returns></returns>
        [Get("/v1/me/player")]
        Task<CurrentlyPlayingContext> GetCurrentPlayback();

        [Get("/v1/me/player/devices")]
        Task<DevicesResponse> GetDevices();
    }
}
