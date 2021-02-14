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
        ///connect-state/v1/connect/transfer/from/288611c1fd1f78c802a810c9e1eb828c0f89b66b/to/8f26dddbc53ab8df72bd2b59671c51605b4efe9
        [Post("/connect-state/v1/{connectType}/{commandType}/from/{from}/to/{to}")]
        Task<HttpResponseMessage> TransferState(string from, string to, 
            string connectType,
            string commandType, 
            [Body] object request);


        [Put("/connect-state/v1/devices/{deviceId}")]
        Task<HttpResponseMessage> PutConnectState(
            [Header("X-Spotify-Connection-Id")]
            string conId,
            string deviceId,
            [Body] PutStateRequest req);
    }
}
