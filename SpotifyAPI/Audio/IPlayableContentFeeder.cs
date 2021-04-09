using System.Threading.Tasks;
using Google.Protobuf;
using JetBrains.Annotations;
using Spotify.Download.Proto;

namespace SpotifyLibrary.Audio
{
    public interface IPlayableContentFeeder
    {
        Task<StorageResolveResponse> ResolveStorageInteractive([NotNull] ByteString fileId,
            bool preload);

        Task<StorageResolveResponse> Fetchoffline([NotNull] ByteString fileid);
    }
}