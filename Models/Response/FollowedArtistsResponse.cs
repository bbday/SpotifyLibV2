using System.Text.Json.Serialization;
using SpotifyLib.Models.Api.Paging;

namespace SpotifyLibV2.Models.Response
{
    public class FollowedArtistsResponse
    {
        [JsonPropertyName("artists")]
        public CursorPaging<FullArtist, FollowedArtistsResponse> Artists { get; set; } = default!;
    }
}
