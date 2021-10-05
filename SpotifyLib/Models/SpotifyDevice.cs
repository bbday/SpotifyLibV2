using System;
using System.Collections.Generic;
using System.Text;
using Connectstate;

namespace SpotifyLib.Models
{
    public interface ISpotifyDevice : IEquatable<ISpotifyDevice>
    {
        DeviceType Type { get; }
        string Name { get; }
        string DeviceId { get; }
        bool CanChangeVolume { get; }
        uint Volume { get; }
        int VolumeSteps { get; }
    }
    public readonly struct RemoteSpotifyDevice : ISpotifyDevice
    {

        public RemoteSpotifyDevice(DeviceInfo name,
            SpotifyConfig config)
        {
            Name = name.Name;
            DeviceId = name.DeviceId;
            IsLocalDevice = name.DeviceId == config.DeviceId;
            CanChangeVolume = name.Capabilities.DisableVolume;
            Type = name.DeviceType;
            Volume = name.Volume;
            VolumeSteps = name.Capabilities.VolumeSteps;
        }

        public DeviceType Type { get; }
        public string Name { get; }
        public string DeviceId { get; }
        public bool IsLocalDevice { get; }
        public bool CanChangeVolume { get; }
        public uint Volume { get; }
        public int VolumeSteps { get; }

        public bool Equals(RemoteSpotifyDevice other)
        {
            return DeviceId == other.DeviceId;
        }
        public bool Equals(ISpotifyDevice other)
        {
            return DeviceId == other.DeviceId;
        }

        public override bool Equals(object obj)
        {
            return obj is RemoteSpotifyDevice other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (DeviceId != null ? DeviceId.GetHashCode() : 0);
        }
    }
}
