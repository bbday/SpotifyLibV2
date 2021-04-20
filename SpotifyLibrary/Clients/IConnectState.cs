using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Refit;
using SpotifyLibrary.Attributes;
using SpotifyLibrary.Models.Response;

namespace SpotifyLibrary.Clients
{
    [ResolvedSpClientEndpoint]
    public interface IConnectState
    {
        [Post("/connect-state/v1/player/command/from/{from}/to/{to}")]
        Task<AcknowledgedResponse> Command(string from, string to,
            [Body(BodySerializationMethod.Serialized)] object command);
        [Post("/connect-state/v1/connect/transfer/from/{from}/to/{to}")]
        Task<AcknowledgedResponse> Transfer(string from, string to,
            [Body(BodySerializationMethod.Serialized)] object options);
    }
}
