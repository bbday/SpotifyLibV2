using System.Threading.Tasks;
using Refit;
using SpotifyLibV2.Attributes;
using SpotifyLibV2.Models.Request;
using SpotifyLibV2.Models.Response;

namespace SpotifyLibV2.Api
{
    [ResolvedSpClientEndpoint]
    public interface IMetadata
    {
        [Get("/playlist/v2/playlist/{plistId}/metadata")]
        Task<PlaylistMetadataProto> GetMetadataForPlaylist(string plistId);

        [Get(
            "/user-profile-view/v3/profile/{userId}")]
        Task<UserProfileResponse> GetUser(string userId, UserRequest request);

        [Get(
            "/user-profile-view/v3/profile/{userId}/followers")]
        Task<ProfilesResponse> GetFollowers(string userId, [AliasAs("from_market")] string market);

        [Get(
            "/user-profile-view/v3/profile/{userId}/following")]
        Task<ProfilesResponse> GetFollowing(string userId, [AliasAs("from_market")] string market);
    }
}