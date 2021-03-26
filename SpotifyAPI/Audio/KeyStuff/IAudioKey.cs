using Google.Protobuf;
using JetBrains.Annotations;
using SpotifyLibrary.Enum;

namespace SpotifyLibrary.Audio.KeyStuff
{
    public interface IAudioKeyManager
    {
        void Dispatch(MercuryPacket packet);
        byte[] GetAudioKey(
            [NotNull] ByteString gid,
            [NotNull] ByteString fileId,
            bool retry = true);
    }
}
