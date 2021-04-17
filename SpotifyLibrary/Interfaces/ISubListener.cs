using SpotifyLibrary.Models;

namespace SpotifyLibrary.Interfaces
{
    public interface ISubListener
    {
        void OnEvent( MercuryResponse resp);
    }
}