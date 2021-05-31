using Spotify.Lib.Models;

namespace Spotify.Lib.Interfaces
{
    public interface ICallback
    {
        internal void Response(MercuryResponse response);
    }
}