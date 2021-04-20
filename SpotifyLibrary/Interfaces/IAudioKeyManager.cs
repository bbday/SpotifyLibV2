using Google.Protobuf;
using SpotifyLibrary.Models;

namespace SpotifyLibrary.Interfaces
{
    public interface IAudioKeyManager
    {
        void Dispatch(MercuryPacket packet);
        byte[] GetAudioKey(
             ByteString gid,
             ByteString fileId,
            bool retry = true);
    }
}
