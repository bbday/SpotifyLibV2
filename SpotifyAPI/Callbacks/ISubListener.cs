using JetBrains.Annotations;
using SpotifyLibrary.Models.Response.Mercury;

namespace SpotifyLibrary.Callbacks
{
    public interface ISubListener
    {
        void OnEvent([NotNull] MercuryResponse resp);
    }
}