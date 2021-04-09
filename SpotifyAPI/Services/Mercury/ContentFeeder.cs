using System;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Google.Protobuf;
using JetBrains.Annotations;
using Spotify.Download.Proto;
using SpotifyLibrary.Audio;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Helpers.Extensions;
using SpotifyLibrary.Services.Mercury.Interfaces;

namespace SpotifyLibrary.Services.Mercury
{
    public class ContentFeeder : IPlayableContentFeeder
    {
        private static readonly string STORAGE_RESOLVE_INTERACTIVE =
            "/storage-resolve/files/audio/interactive";
        private static readonly string STORAGE_RESOLVE_INTERACTIVE_PREFETCH =
            "/storage-resolve/files/audio/interactive_prefetch";

        private static readonly string STORAGE_RESOLVE_OFFLINE
            = "/storage-resolve/v2/files/audio/offline/1";

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
                .WithOAuthBearerToken((await _tokens.GetToken("playlist-read")).AccessToken)
                .GetBytesAsync();

            if (resp == null) throw new Exception("Response body is empty!");
            return StorageResolveResponse.Parser.ParseFrom(resp);
        }

        public async Task<StorageResolveResponse> Fetchoffline(ByteString fileId)
        {
            var resp = await(await ApResolver.GetClosestSpClient())
                .AppendPathSegment(STORAGE_RESOLVE_OFFLINE)
                .AppendPathSegment(fileId.ToByteArray().BytesToHex())
                .WithOAuthBearerToken((await _tokens.GetToken("playlist-read")).AccessToken)
                .GetBytesAsync();

            if (resp == null) throw new Exception("Response body is empty!");
            return StorageResolveResponse.Parser.ParseFrom(resp);
        }
    }
}
