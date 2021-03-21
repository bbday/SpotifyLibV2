using Google.Protobuf;
using JetBrains.Annotations;
using SpotifyLibV2.Mercury;

namespace SpotifyLibV2.Audio.Decrypt
{
    public interface IAudioKey
    {
        void Dispatch(MercuryPacket packet);
        byte[] GetAudioKey(
            [NotNull] ByteString gid,
            [NotNull] ByteString fileId,
            bool retry = true);
    }
}
