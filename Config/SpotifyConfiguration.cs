#nullable enable
using System;
using Connectstate;
using SpotifyLibV2.Audio;
using SpotifyLibV2.Helpers;
using SpotifyLibV2.Helpers.Extensions;
using SpotifyLibV2.Models.Public;

namespace SpotifyLibV2.Config
{
    public class SpotifyConfiguration
    {
        public static SpotifyConfiguration Default()
        {
            return new("Ongaku PC", 
                DeviceType.Computer, 
                "en",
                null);
        }

        public SpotifyConfiguration(string deviceName,
            DeviceType deviceType, 
            string preferredLocale,
            Func<StoredCredentials, string>? storeCredentialsFunction = null,
            string? deviceId = null)
        {
            if (deviceId != null && !deviceId.IsHexString(40))
            {
                throw new Exception("Invalid device id.");
            }

            StoreCredentials = storeCredentialsFunction != null;
#pragma warning disable CS8601 // Possible null reference assignment.
            StoreCredentialsFunction = storeCredentialsFunction;
#pragma warning restore CS8601 // Possible null reference assignment.
            DeviceType = deviceType;
            PreferredLocale = preferredLocale;
            DeviceName = deviceName;
            DeviceId = deviceId ?? Utils.RandomHexString(40).ToLower();
            PrefereAudioQuality = AudioQualityHelper.AudioQuality.NORMAL;
        }
        public readonly Func<StoredCredentials, string>? StoreCredentialsFunction;
        public readonly bool StoreCredentials;
        public DeviceType DeviceType
        {
            get; private set;
        }
        public string DeviceName
        {
            get; private set;
        }
        public string PreferredLocale
        {
            get; private set;
        }
        public string Country { get; internal set; }
        public string DeviceId { get; private set; }

        public AudioQualityHelper.AudioQuality PrefereAudioQuality
        {
            get; set;
        }
    }
}
