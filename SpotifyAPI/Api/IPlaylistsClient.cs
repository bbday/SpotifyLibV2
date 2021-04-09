using System.Threading.Tasks;
using Refit;
using SpotifyLibrary.Attributes;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Request;
using SpotifyLibrary.Models.Response;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Api
{

    [OpenUrlEndpoint]
    public interface IPlaylistsClient
    {
        [Get("/v1/playlists/{id}")]
        Task<FullPlaylist> GetPlaylist(string id, PlaylistRequest playlistRequest);

        [Post("/v1/playlists/{id}/tracks")]
        Task<RevisionId> AddTracks(string id, AddTracksRequest request);
    }
}