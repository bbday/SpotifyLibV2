using JetBrains.Annotations;
using SpotifyLibrary.Models.Response;
using SpotifyLibrary.Models.Response.Mercury;

namespace SpotifyLibrary.Callbacks
{
    public interface ICallback
    {
        internal void Response([NotNull] MercuryResponse response);
    }
}