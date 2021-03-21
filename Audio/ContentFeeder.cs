using System;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Google.Protobuf;
using JetBrains.Annotations;
using Spotify.Download.Proto;
using SpotifyLibV2.Api;
using SpotifyLibV2.Helpers.Extensions;

namespace SpotifyLibV2.Audio
{
    public class ContentFeeder : IPlayableContentFeeder
    {
        private static readonly string STORAGE_RESOLVE_INTERACTIVE = 
            "/storage-resolve/files/audio/interactive";
        private static readonly string STORAGE_RESOLVE_INTERACTIVE_PREFETCH =
            "/storage-resolve/files/audio/interactive_prefetch";

        private readonly ITokensProvider _tokens;
        public ContentFeeder(ITokensProvider tokens)
        {
            _tokens = tokens;
        }

        public async Task<StorageResolveResponse> ResolveStorageInteractive([NotNull] ByteString fileId,
            bool preload)
        {
            var resp = await (await ApResolver.GetClosestSpClient())
                .AppendPathSegment(preload ? STORAGE_RESOLVE_INTERACTIVE_PREFETCH : STORAGE_RESOLVE_INTERACTIVE)
                .AppendPathSegment(fileId.ToByteArray().BytesToHex())
                .WithOAuthBearerToken(_tokens.GetToken("playlist-read").AccessToken)
                .GetBytesAsync();

            if (resp == null) throw new Exception("Response body is empty!");
            return StorageResolveResponse.Parser.ParseFrom(resp);
        }
    }
}
