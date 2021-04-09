using System;
using System.Threading.Tasks;
using Connectstate;
using SpotifyLibrary.Authentication;
using SpotifyLibrary.Helpers;

namespace SpotifyLibrary.Configs
{
    public class SpotifyConfiguration
    {
        public readonly Func<StoredCredentials, Task<string>>? StoreCredentialsFunction;

        /// <summary>
        ///     Creates a new <see cref="SpotifyConfiguration" /> config.
        /// </summary>
        /// <param name="authenticator">Type of authentication</param>
        /// <param name="locale"></param>
        /// <param name="restRetryCount">Number of times to retry a HTTPs call. Default : 3</param>
        /// <param name="deviceName"></param>
        /// <param name="deviceType"></param>
        /// <param name="retryTimeOutWaiter">
        ///     Function that takes input retry count and should output how much to wait.
        ///     Default: Static 2 seconds
        /// </param>
        public SpotifyConfiguration(IAuthenticator authenticator,
            string deviceName, DeviceType deviceType = DeviceType.Computer,
            string locale = "en",
            int restRetryCount = 3,
            Func<int, TimeSpan>? retryTimeOutWaiter = null,
            Func<StoredCredentials, Task<string>>? storeCredentialsFunction = null,
            TimeSpan? maxTimeout = null,
            string sqlPath = null)
        {
            StoreCredentials = storeCredentialsFunction != null;
            StoreCredentialsFunction = storeCredentialsFunction;
            retryTimeOutWaiter ??= i => TimeSpan.FromSeconds(2);
            RestRetryCont = restRetryCount;
            RetryTimeoutWaiter = retryTimeOutWaiter;
            Locale = locale;
            DeviceName = deviceName;
            MaxTimeout = maxTimeout ?? TimeSpan.FromSeconds(5);
            DeviceType = deviceType;
            DeviceId = Utils.RandomHexString(40).ToLower();
            Authenticator = authenticator;
            SqlPath = sqlPath;
        }
        public string SqlPath { get; }
        public int RestRetryCont { get; }
        public Func<int, TimeSpan> RetryTimeoutWaiter { get; }
        public IAuthenticator Authenticator { get; }
        public string Locale { get; }
        public string DeviceId { get; }
        public string DeviceName { get; }
        public DeviceType DeviceType { get; }
        public TimeSpan MaxTimeout { get; }
        public bool StoreCredentials { get; }
        public bool AutoplayEnabled { get; set; }
    }
}