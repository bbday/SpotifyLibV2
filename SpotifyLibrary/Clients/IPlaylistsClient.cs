using System.Threading.Tasks;
using Refit;
using SpotifyLibrary.Attributes;
using SpotifyLibrary.Ids;
using SpotifyLibrary.Models.Requests;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Clients
{
    [OpenUrlEndpoint]
    public interface IPlaylistsClient
    {
        [Get("/v1/playlists/{id}")]
        Task<FullPlaylist> GetPlaylist(string id, PlaylistRequest playlistRequest);
    }
}