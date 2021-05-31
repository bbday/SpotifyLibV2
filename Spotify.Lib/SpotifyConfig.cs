using Spotify.Lib.Helpers;

namespace Spotify.Lib
{
    public static class SpotifyConfig
    {
        public static string DeviceId { get; internal set; } = Utils.RandomHexString(40).ToLower();

        public static string Locale { get; internal set; } = "en";
        public static string DeviceName { get; internal set; } = "Ongaku";
    }
}