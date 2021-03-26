using System.Threading.Tasks;
using Google.Protobuf;
using Spotify;

namespace SpotifyLibrary.Authentication
{
    public class UserPassAuthenticator : IAuthenticator
    {
        private readonly LoginCredentials credentials;

        /// <summary>
        ///     Authenticate based on username and password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="passwordSecureString"></param>
        public UserPassAuthenticator(string username,
            string passwordSecureString)
        {
            credentials = new LoginCredentials
            {
                Username = username,
                AuthData = ByteString.CopyFromUtf8(passwordSecureString),
                Typ = AuthenticationType.AuthenticationUserPass
            };
        }

        public Task<LoginCredentials> Get()
        {
            return Task.FromResult(credentials);
        }
    }
}