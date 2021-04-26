using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Newtonsoft.Json;
using Spotify;
using SpotifyLibrary.Interfaces;
using SpotifyLibrary.Models;

namespace SpotifyLibrary.Authenticators
{
    public class StoredAuthenticator : IAuthenticator
    {
        private readonly Func<Task<StoredCredentials>> fetch;
        private LoginCredentials credentials;


        /// <summary>
        ///     Authenticate based on stored credentials
        /// </summary>
        /// <param name="fetchJsonData">
        ///     A function that should return a json format of type <see cref="StoredCredentials" />
        ///     You can use this parameter to for example fetch a file and read its contents.
        /// </param>
        public StoredAuthenticator(Func<Task<StoredCredentials>> fetchJsonData)
        {
            fetch = fetchJsonData;
        }

        public async Task<LoginCredentials> Get()
        {
            var data = await fetch.Invoke();
            if (data == null)
                throw new UnauthorizedAccessException("No credentials stored.");
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