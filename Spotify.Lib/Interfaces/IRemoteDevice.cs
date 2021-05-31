using System;

namespace Spotify.Lib.Interfaces
{
    public interface IRemoteDevice : IEquatable<IRemoteDevice>
    {
        string Id { get; }
        string DisplayName { get; }
        string DeviceType { get; }
        bool AllowVolume { get; set; }
        int Volume { get; set; }
        bool IsActive { get; set; }
    }
}