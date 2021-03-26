using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Newtonsoft.Json;
using Spotify;
using SpotifyLibrary.Configs;

namespace SpotifyLibrary.Authentication
{
    public class StoredAuthenticator : IAuthenticator
    {
        private readonly Func<Task<string>> fetch;
        private LoginCredentials credentials;


        /// <summary>
        ///     Authenticate based on stored credentials
        /// </summary>
        /// <param name="fetchJsonData">
        ///     A function that should return a json format of type <see cref="StoredCredentials" />
        ///     You can use this parameter to for example fetch a file and read its contents.
        /// </param>
        public StoredAuthenticator(Func<Task<string>> fetchJsonData)
        {
            fetch = fetchJsonData;
        }

        public async Task<LoginCredentials> Get()
        {
            var json = await fetch.Invoke();
            if (string.IsNullOrEmpty(json))
                throw new UnauthorizedAccessException("No credentials stored.");
            var data = JsonConvert.DeserializeObject<StoredCredentials>(json);
            credentials = new LoginCredentials
            {
                Typ = data.AuthenticationType,
                Username = data.Username,
                AuthData = ByteString.FromBase64(data.Base64Credentials)
            };
            return credentials;
        }
    }
}