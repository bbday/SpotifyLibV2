using System.Net.Http;
using System.Threading.Tasks;
using Connectstate;
using Refit;
using SpotifyLibV2.Attributes;
using SpotifyLibV2.Models.Request;

namespace SpotifyLibV2.Api
{
    [ResolvedSpClientEndpoint]
    public interface IConnectState
    {
        [Post("/connect-state/v1/player/command/from/{from}/to/{to}")]
        Task<HttpResponseMessage> TransferState(string from, string to, [Body] object request);


        [Put("/connect-state/v1/devices/{deviceId}")]
        Task<HttpResponseMessage> PutConnectState(
            [Header("X-Spotify-Connection-Id")] string conId,
            string deviceId,
            [Body] PutStateRequest req);
    }
}
