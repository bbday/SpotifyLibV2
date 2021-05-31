using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Spotify.Lib.Interfaces;

namespace Spotify.Lib.Models
{
    public class SpotifyDevice : IRemoteDevice, INotifyPropertyChanged
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

        public event PropertyChangedEventHandler PropertyChanged;
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
            if (obj.GetType() != GetType()) return false;
            return Equals((SpotifyDevice) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, DeviceType);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}