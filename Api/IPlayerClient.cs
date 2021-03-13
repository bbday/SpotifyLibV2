using System.Threading.Tasks;
using Refit;
using SpotifyLibV2.Attributes;
using SpotifyLibV2.Models.Request;
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
        Task<CurrentlyPlayingContext> GetCurrentPlayback([AliasAs("additional_types")]string addtypes = "track,episode");

        [Get("/v1/me/player/devices")]
        Task<DevicesResponse> GetDevices();  
        /// <summary>
        /// Set the volume for the user’s current playback device.
        /// </summary>
        /// <param name="request">The request-model which contains required and optional parameters.</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-set-volume-for-users-playback
        /// </remarks>
        /// <returns></returns>
        [Put("/v1/me/player/volume")]
        Task<bool> SetVolume(PlayerVolumeRequest request);
    }
}
