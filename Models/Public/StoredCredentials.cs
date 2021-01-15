using Spotify;

namespace SpotifyLibV2.Models.Public
{
    public class StoredCredentials
    {
        public string Username { get; set; }
        public string Base64Credentials { get; set; }
        public AuthenticationType AuthenticationType { get; set; }
    }
}

