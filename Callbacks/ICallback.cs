using JetBrains.Annotations;
using SpotifyLibV2.Mercury;

namespace SpotifyLibV2.Listeners
{
    public interface ICallback
    {
        internal void Response([NotNull] MercuryResponse response);
    }
}

