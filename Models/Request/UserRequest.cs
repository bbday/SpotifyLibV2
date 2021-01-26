using Refit;

namespace SpotifyLibV2.Models.Request
{
    public class UserRequest
    {
        public UserRequest(int playlistLimit, int artistLimti, string market = "from_token")
        {
            PlaylistLimit = playlistLimit;
            ArtistLimit = artistLimti;
            Market = market;
        }


        [AliasAs("playlist_limit")]
        public int PlaylistLimit { get; set; }


        [AliasAs("artist_limit")]
        public int ArtistLimit { get; set; }


        [AliasAs("market")]
        public string? Market { get; set; }
    }
}