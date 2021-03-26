using SpotifyLibrary.Models.Enums;

namespace SpotifyLibrary.Models
{
    public interface IRemoteDevice
    {
        AudioService Type { get; }
        string Id { get; }
        string DisplayName { get; }
        string DeviceType { get; }
        bool AllowVolume { get; set; }
        int Volume { get; set; }
        bool IsActive { get; set; }
    }
}
