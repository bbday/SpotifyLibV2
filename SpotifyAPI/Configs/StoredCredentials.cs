using Spotify;

namespace SpotifyLibrary.Configs
{
    public class StoredCredentials
    {
        public string Username { get; set; }
        public string Base64Credentials { get; set; }
        public AuthenticationType AuthenticationType { get; set; }
    }
}