using System.IO;

namespace SpotifyLibV2.Audio.Decrypt
{
    public interface IAudioDecrypt
    {
        void DecryptChunk(int chunkIndex, byte[] buffer, int size = 0);
        int DecryptTimeMs();
    }
}
