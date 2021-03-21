using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using JetBrains.Annotations;
using Spotify.Download.Proto;

namespace SpotifyLibV2.Audio
{
    public interface IPlayableContentFeeder
    {
        Task<StorageResolveResponse> ResolveStorageInteractive([NotNull] ByteString fileId,
            bool preload);
    }
}
