using System.Threading.Tasks;
using Refit;
using SpotifyLibrary.Attributes;
using SpotifyLibrary.Models.Response;

namespace SpotifyLibrary.Api
{
    [ResolvedSpClientEndpoint]
    public interface IConnectState
    {
        [Post("/connect-state/v1/player/command/from/{from}/to/{to}")]
        Task<AcknowledgedResponse> Command(string from, string to, 
            [Body(BodySerializationMethod.Serialized)]object command);
    }
}
