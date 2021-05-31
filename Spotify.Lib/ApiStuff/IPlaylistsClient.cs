using System.Threading.Tasks;
using Refit;
using Spotify.Lib.Attributes;
using Spotify.Lib.Models.Ids;
using Spotify.Lib.Models.Request;
using Spotify.Lib.Models.Response.SpotItems.FullItems;
using Spotify.Lib.Models.Response.SpotItems.SimpleItems;

namespace Spotify.Lib.ApiStuff
{
    [OpenUrlEndpoint]
    public interface IPlaylistsClient
    {
        [Get("/v1/playlists/{id}")]
        Task<FullPlaylist> GetPlaylist(string id, PlaylistRequest playlistRequest);
    }
}