using Spotify.Lib.Models;

namespace Spotify.Lib.Interfaces
{
    public interface ISubListener
    {
        void OnEvent(MercuryResponse resp);
    }
}