using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Playlists;

namespace SpotifyLibrary
{
    public interface IPlaylistListener
    {
        void PlaylistUpdate(HermesPlaylistUpdate update);
        void RootlistUpdate(RevisionId revision);
    }
}
