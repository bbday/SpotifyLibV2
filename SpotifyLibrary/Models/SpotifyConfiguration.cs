using System;
using Connectstate;
using ReactiveUI;
using SpotifyLibrary.Bases;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Models;

namespace SpotifyLibrary.Configs
{
    public class SpotifyConfiguration : ViewModelBase
    {
        private int _maxHttpRetries;
        private int _maxMercuryRetries;

        public SpotifyConfiguration(string deviceId,
            Action<StoredCredentials>? storeCredentialsFunction = null,
            string locale = "en",
            int maxHttpRetries = 3,
            int maxMercuryRetries = 3,
            DeviceType deviceType = DeviceType.Computer,
            string deviceName = "Ongaku")
        {
            DeviceId = deviceId;
            StoreCredentialsFunction = storeCredentialsFunction;
            Locale = locale;
            MaxMercuryRetries = maxMercuryRetries;
            MaxHttpRetries = maxHttpRetries;
            DeviceType = deviceType;
            DeviceName = deviceName;
        }

        public static SpotifyConfiguration Default()
        {
            return new SpotifyConfiguration(Utils.RandomHexString(40).ToLower());
        }



        public int MaxMercuryRetries
        {
            get => _maxMercuryRetries;
            set => this.RaiseAndSetIfChanged(ref _maxMercuryRetries, value);
        }
        public int MaxHttpRetries
        {
            get => _maxHttpRetries;
            set => this.RaiseAndSetIfChanged(ref _maxHttpRetries, value);
        }

        public string DeviceId { get; }
        public string Locale { get; }

        public bool StoreCredentials => StoreCredentialsFunction != null;
        public Action<StoredCredentials>? StoreCredentialsFunction { get; set; }
        public DeviceType DeviceType { get;  }
        public string DeviceName { get; }
    }
}
