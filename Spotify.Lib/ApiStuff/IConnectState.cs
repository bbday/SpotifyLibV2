using System.Threading.Tasks;
using Refit;
using Spotify.Lib.Attributes;
using Spotify.Lib.Models.Response;

namespace Spotify.Lib.ApiStuff
{
    [ResolvedSpClientEndpoint]
    public interface IConnectState
    {
        [Post("/connect-state/v1/player/command/from/{from}/to/{to}")]
        Task<AcknowledgedResponse> Command(string from, string to,
            [Body(BodySerializationMethod.Serialized)]
            object command);

        [Post("/connect-state/v1/connect/transfer/from/{from}/to/{to}")]
        Task<AcknowledgedResponse> Transfer(string from, string to,
            [Body(BodySerializationMethod.Serialized)]
            object options);
    }
}