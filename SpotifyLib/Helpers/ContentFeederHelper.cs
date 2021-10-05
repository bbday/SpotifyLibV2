using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Google.Protobuf;
using Spotify.Download.Proto;

namespace SpotifyLib.Helpers
{
    public static class ContentFeederHelper
    {
        private const string STORAGE_RESOLVE_INTERACTIVE =
            "/storage-resolve/files/audio/interactive";
        private const string STORAGE_RESOLVE_INTERACTIVE_PREFETCH =
            "/storage-resolve/files/audio/interactive_prefetch";
        private const string STORAGE_RESOLVE_OFFLINE
            = "/storage-resolve/v2/files/audio/offline/1";

        public static async Task<StorageResolveResponse> ResolveStorageInteractive(
            this SpotifyConnectionState connState,
            ByteString fileId,
            bool preload,
            CancellationToken ct)
        {
            var resp = await (await ApResolver.GetClosestSpClient())
                .AppendPathSegment(preload ? STORAGE_RESOLVE_INTERACTIVE_PREFETCH : STORAGE_RESOLVE_INTERACTIVE)
                .AppendPathSegment(fileId.ToByteArray().BytesToHex())
                .WithOAuthBearerToken((await connState.GetToken(ct)).AccessToken)
                .GetBytesAsync(ct);

            if (resp == null) throw new Exception("Response body is empty!");
            return StorageResolveResponse.Parser.ParseFrom(resp);
        }
    }
}
