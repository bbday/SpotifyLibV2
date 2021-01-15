using JetBrains.Annotations;
using SpotifyLibV2.Mercury;

namespace SpotifyLibV2.Listeners
{
    public interface ISubListener
    {
        void OnEvent([NotNull] MercuryResponse resp);
    }
}
