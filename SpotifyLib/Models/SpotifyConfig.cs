using SpotifyLib.Helpers;

namespace SpotifyLib.Models
{
    public readonly struct SpotifyConfig
    {
        public SpotifyConfig(string locale, string deviceId,
            string deviceName = "Ongaku")
        {
            Locale = locale;
            DeviceId = deviceId;
            DeviceName = deviceName;
        }

        public static SpotifyConfig Default()
        {
            return new SpotifyConfig("en", Utils.RandomHexString(40).ToLower());
        }

        public string Locale { get; }
        public string DeviceId { get; }
        public string DeviceName { get; }
    }
}
