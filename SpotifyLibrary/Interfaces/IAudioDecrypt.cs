namespace SpotifyLibrary.Interfaces
{
    public interface IAudioDecrypt
    {
        void DecryptChunk(int chunkIndex, byte[] buffer, int size = 0);
        int DecryptTimeMs();
    }
}

