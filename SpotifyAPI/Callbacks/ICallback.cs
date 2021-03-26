using JetBrains.Annotations;
using SpotifyLibrary.Models.Response;

namespace SpotifyLibrary.Callbacks
{
    public interface ICallback
    {
        internal void Response([NotNull] MercuryResponse response);
    }
}