using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;
using SpotifyLibrary.Bases;

namespace SpotifyLibrary.Models.Response
{
    public class SpotifyDevice : ViewModelBase, IRemoteDevice
    {
        public SpotifyDevice(string id, string name, string deviceType, bool allowVolume, int volume, bool isActive)
        {
            Id = id;
            DisplayName = name;
            DeviceType = deviceType;
            AllowVolume = allowVolume;
            Volume = volume;
            IsActive = isActive;
        }
        public AudioServiceType Type => AudioServiceType.Spotify;
        public string Id { get; }
        public string DisplayName { get; }
        public string DeviceType { get; }
        public bool AllowVolume { get; set; }
        public int Volume { get; set; }
        public bool IsActive { get; set; }

        public bool Equals(IRemoteDevice? other)
        {
            if (other == null) return false;
            return Id == other.Id && DeviceType == other.DeviceType;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SpotifyDevice) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, DeviceType);
        }
    }
}
