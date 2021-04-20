using System;
using MediaLibrary.Enums;

namespace MediaLibrary.Interfaces
{
    public interface IRemoteDevice : IEquatable<IRemoteDevice>
    {
        AudioServiceType Type { get; }
        string Id { get; }
        string DisplayName { get; }
        string DeviceType { get; }
        bool AllowVolume { get; set; }
        int Volume { get; set; }
        bool IsActive { get; set; }
    }
}
