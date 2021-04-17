using SpotifyLibrary.Models;

namespace SpotifyLibrary.Interfaces
{
    public interface ICallback
    {
        internal void Response(MercuryResponse response);
    }
}