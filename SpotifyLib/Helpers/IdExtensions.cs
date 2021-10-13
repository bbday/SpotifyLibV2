using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Google.Protobuf;
using SpotifyLib.Models;

namespace SpotifyLib.Helpers
{
    public static class IdExtensions
    {
        public static async Task<T> FetchAsync<T>(this SpotifyId id,
            ISpotifyConnectionState state,
            CancellationToken ct)
            where T : IMessage, new()
        {
            var d = await (await ApResolver.GetClosestSpClient())
                .AppendPathSegments("metadata", "4", id.Type.ToString().ToLowerInvariant(),
                    id.ToHexId().ToLowerInvariant())
                .WithOAuthBearerToken((await state.GetToken(ct)).AccessToken)
                .GetBytesAsync(ct);
            var message = new T();
            message.MergeFrom(d);
            return message;
        }
    }
}
