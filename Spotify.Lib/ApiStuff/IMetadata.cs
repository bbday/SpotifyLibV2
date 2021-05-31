using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using Spotify.Lib.Attributes;

namespace Spotify.Lib.ApiStuff
{
    [ResolvedSpClientEndpoint]
    public interface IMetadataClient
    {
        [Get("/playlist/v2/playlist/{plistId}")]
        Task<HttpResponseMessage> GetPlaylistWithContent(string plistId, [Header("Accept")] string accept);
        [Get("/playlist/v2/playlist/{plistId}/metadata")]
        Task<HttpResponseMessage> GetMetadataForPlaylist(string plistId);

        [Get("/playlist/v2/playlist/{playlistId}/diff?revision={revision}&handlesContent=")]
        Task<HttpResponseMessage> GetDiff(string playlistId, string revision);

        [Get("/playlist/v2/user/{userId}/rootlist?decorate={decoration}")]
        Task<HttpResponseMessage>GetUserLists(string userId, string decoration);

    }
}
