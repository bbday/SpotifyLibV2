using JetBrains.Annotations;
using SpotifyLibV2.Enums;
using SpotifyLibV2.Models;

namespace SpotifyLibV2.Connect.Interfaces
{
    internal interface IDeviceStateListener
    {
        void Ready();

        void Command([NotNull] Endpoint endpoint, [NotNull] CommandBody data);

        void VolumeChanged();

        void NotActive();
    }
}
