namespace SpotifyLibrary.Interfaces
{
    public interface IKeyCallBack
    {
        public void Key(byte[] key);

        public void Error(short code);
    }
}