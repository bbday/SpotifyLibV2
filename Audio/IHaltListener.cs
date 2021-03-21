namespace SpotifyLibV2.Audio
{
    public interface IHaltListener
    {
        void StreamReadHalted(int chunk, long time);

        void StreamReadResumed(int chunk, long time);
    }
}
